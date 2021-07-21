using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Packaging;
using OpenXmlPowerTools;

namespace OpenXmlPowerTools
{
    public class BookmarkReplacerException : Exception
    {
        public BookmarkReplacerException(string message)
            : base(message)
        {
        }
    }

    public class BookmarkReplacer
    {
        private static XNamespace BookmarkReplacerCustomNamespace =
            "http://powertools.codeplex.com/2011/bookmarkreplacer";

        private static object FlattenParagraphsTransform(XNode node)
        {
            XElement element = node as XElement;
            if (element != null)
            {
                if (element.Name == W.p)
                {
                    return element
                        .Elements()
                        .Where(e => e.Name != W.pPr)
                        .Select(e => FlattenParagraphsTransform(e))
                        .Concat(
                            new[]
                        {
                            new XElement(W.p,
                                element.Attributes(),
                                element.Elements(W.pPr))
                        });
                }
                return new XElement(element.Name,
                    element.Attributes(),
                    element.Nodes().Select(n => FlattenParagraphsTransform(n)));
            }
            return node;
        }

        private struct BlockLevelState
        {
            public int Index;
            public bool BlockLevelElementBefore;
        };

        private static object UnflattenParagraphsTransform(XNode node)
        {
            XElement element = node as XElement;
            if (element != null)
            {
                if (element.Elements().Any(e => e.Name == W.p))
                {
                    var paraIndex = element
                        .Elements()
                        .Rollup(
                            new BlockLevelState()
                            {
                                Index = 0,
                                BlockLevelElementBefore = false,
                            },
                            (e2, s) =>
                            {
                                if (s.BlockLevelElementBefore)
                                    return new BlockLevelState
                                    {
                                        Index = s.Index + 1,
                                        BlockLevelElementBefore =
                                            (e2.Name == W.p ||
                                             e2.Name == W.tbl ||
                                             e2.Name == W.tcPr ||
                                            (e2.Name == W.sdt &&
                                             e2.Descendants(W.p).Any())),
                                    };
                                return new BlockLevelState
                                {
                                    Index = s.Index,
                                    BlockLevelElementBefore =
                                        (e2.Name == W.p ||
                                         e2.Name == W.tbl ||
                                         e2.Name == W.tcPr ||
                                        (e2.Name == W.sdt &&
                                         e2.Descendants(W.p).Any())),
                                };
                            });
                    var zipped = element.Elements().Zip(paraIndex, (a, b) =>
                        new
                        {
                            Element = a,
                            ParaIndex = b,
                        });

                    var grouped = zipped
                        .GroupAdjacent(e3 => e3.ParaIndex.Index);
                    var newElements = grouped
                        .Select(g =>
                        {
                            XElement lastElement = g.Last().Element;
                            if (lastElement.Name != W.p)
                                return (object)g.Select(gc =>
                                    UnflattenParagraphsTransform(gc.Element));
                            int count = g.Count();
                            XElement newParagraph = new XElement(W.p,
                                lastElement.Attributes(),
                                g.Take(count - 1)
                                    .Select(e4 =>
                                        UnflattenParagraphsTransform(e4.Element)));
                            return newParagraph;
                        });
                    XElement newElement = new XElement(element.Name,
                        element.Attributes(),
                        newElements);
                    return newElement;
                }
                return new XElement(element.Name,
                    element.Attributes(),
                    element.Nodes().Select(n => UnflattenParagraphsTransform(n)));
            }
            return node;
        }

        private static object ReplaceInsertElement(XNode node, string replacementText)
        {
            XElement element = node as XElement;
            if (element != null)
            {
                if (element.Name == BookmarkReplacerCustomNamespace + "Insert")
                {
                    XName parentName = element.Parent.Name;
                    if (parentName == W.body || parentName == W.tc ||
                        parentName == W.txbxContent)
                    {
                        return new XElement(W.p,
                            new XElement(W.r,
                                element.Elements(),
                                new XElement(W.t, replacementText)));
                    }
                    return new XElement(W.r,
                        element.Elements(),
                        new XElement(W.t, replacementText));
                }
                return new XElement(element.Name,
                    element.Attributes(),
                    element.Nodes().Select(n => ReplaceInsertElement(n, replacementText)));
            }
            return node;
        }

        private static object DemoteRunChildrenOfBodyTransform(XNode node)
        {
            XElement element = node as XElement;
            if (element != null)
            {
                if (element.Name == W.r && element.Parent.Name == W.body)
                    return new XElement(W.p, element);
                return new XElement(element.Name,
                    element.Attributes(),
                    element.Nodes().Select(n => DemoteRunChildrenOfBodyTransform(n)));
            }
            return node;
        }

        public static void ReplaceBookmarkText(WordprocessingDocument doc,
            string bookmarkName, string replacementText)
        {
            XDocument xDoc = doc.MainDocumentPart.GetXDocument();
            XElement bookmark = xDoc.Descendants(W.bookmarkStart)
                .FirstOrDefault(d =>
                    (string)d.Attribute(W.name) == bookmarkName);
            if (bookmark == null)
                throw new BookmarkReplacerException(
                    "Document doesn't contain bookmark.");
            if (bookmark.Parent.Name.Namespace == M.m)
                throw new BookmarkReplacerException(
                    "Replacing text in math formulas is not supported.");
            if (RevisionAccepter.HasTrackedRevisions(doc))
                throw new BookmarkReplacerException(
    "Replacing bookmark text in documents that have tracked revisions is not supported.");
            if (xDoc.Descendants(W.sdt).Any())
                throw new BookmarkReplacerException(
    "Replacing bookmark text in documents that have content controls is not supported.");
            XElement newRoot = (XElement)FlattenParagraphsTransform(xDoc.Root);
            XElement startBookmarkElement = newRoot.Descendants(W.bookmarkStart)
                .Where(d => (string)d.Attribute(W.name) == bookmarkName)
                .FirstOrDefault();
            int bookmarkId = (int)startBookmarkElement.Attribute(W.id);
            XElement endBookmarkElement = newRoot.Descendants(W.bookmarkEnd)
                .Where(d => (int)d.Attribute(W.id) == bookmarkId)
                .FirstOrDefault();
            if (startBookmarkElement.Ancestors(W.hyperlink).Any() ||
                endBookmarkElement.Ancestors(W.hyperlink).Any())
                throw new BookmarkReplacerException(
                    "Bookmark is within a hyperlink.  Can't replace text.");
            if (startBookmarkElement.Ancestors(W.fldSimple).Any() ||
                endBookmarkElement.Ancestors(W.fldSimple).Any())
                throw new BookmarkReplacerException(
                    "Bookmark is within a simple field.  Can't replace text.");
            if (startBookmarkElement.Ancestors(W.smartTag).Any() ||
                endBookmarkElement.Ancestors(W.smartTag).Any())
                throw new BookmarkReplacerException(
                    "Bookmark is within a smart tag.  Can't replace text.");
            if (startBookmarkElement.Parent != endBookmarkElement.Parent)
                throw new BookmarkReplacerException(
                    "Bookmark start and end not at same levels.  Can't replace text.");

            XElement parentElement = startBookmarkElement.Parent;
            var elementsBetweenBookmarks = startBookmarkElement
                .ElementsAfterSelf()
                .TakeWhile(e => e != endBookmarkElement);
            var newElements = parentElement
                .Elements()
                .TakeWhile(e => e != startBookmarkElement)
                .Concat(new[]
                {
                    startBookmarkElement,
                    new XElement(BookmarkReplacerCustomNamespace + "Insert",
                        elementsBetweenBookmarks
                            .Where(e => e.Name == W.r)
                            .Take(1)
                            .Elements(W.rPr)
                            .FirstOrDefault()),
                })
                .Concat(elementsBetweenBookmarks.Where(e => e.Name != W.p &&
                    e.Name != W.r && e.Name != W.tbl))
                .Concat(new[]
                {
                    endBookmarkElement
                })
                .Concat(endBookmarkElement.ElementsAfterSelf());
            parentElement.ReplaceNodes(newElements);

            newRoot = (XElement)UnflattenParagraphsTransform(newRoot);
            newRoot = (XElement)ReplaceInsertElement(newRoot, replacementText);
            newRoot = (XElement)DemoteRunChildrenOfBodyTransform(newRoot);

            xDoc.Elements().First().ReplaceWith(newRoot);
            doc.MainDocumentPart.PutXDocument();
        }

        public static string GetBookmarkText(WordprocessingDocument doc, string bookmarkName)
        {
            XDocument xDoc = doc.MainDocumentPart.GetXDocument();
            bool containsBookmark = xDoc.Descendants(W.bookmarkStart)
                .Where(d => (string)d.Attribute(W.name) == bookmarkName)
                .Any();
            if (!containsBookmark)
                throw new BookmarkReplacerException(
                    "Document doesn't contain bookmark.");
            XElement newRoot = (XElement)FlattenParagraphsTransform(xDoc.Root);

            XElement startBookmarkElement = newRoot.Descendants(W.bookmarkStart)
                .Where(d => (string)d.Attribute(W.name) == bookmarkName)
                .FirstOrDefault();
            int bookmarkId = (int)startBookmarkElement.Attribute(W.id);
            XElement endBookmarkElement = newRoot.Descendants(W.bookmarkEnd)
                .Where(d => (int)d.Attribute(W.id) == bookmarkId)
                .FirstOrDefault();
            if (startBookmarkElement.Parent != endBookmarkElement.Parent)
                throw new BookmarkReplacerException(
                    "Bookmark start and end not at same levels.  Can't retrieve text.");

            XElement parentElement = startBookmarkElement.Parent;
            var elementsBetweenBookmarks = startBookmarkElement
                .ElementsAfterSelf()
                .TakeWhile(e => e != endBookmarkElement);
            var text = elementsBetweenBookmarks
                .Select(e =>
                {
                    if (e.Name == W.r)
                    {
                        string runText = e.Descendants(W.t)
                            .Select(t => (string)t).StringConcatenate();
                        return runText;
                    }
                    if (e.Name == W.p)
                        return Environment.NewLine;
                    return "";
                })
                .StringConcatenate();
            return text;
        }
    }
}
