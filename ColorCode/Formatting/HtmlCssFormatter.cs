using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using ColorCode.Common;
using ColorCode.Parsing;

namespace ColorCode.Formatting
{
	public class HtmlCssFormatter : IFormatter
	{
		public void Write(string parsedSourceCode,
						  IList<Scope> scopes,
						  IStyleSheet _,
						  TextWriter textWriter) {
			var styleInsertions = new List<TextInsertion>();

			foreach (Scope scope in scopes)
				GetStyleInsertionsForCapturedStyle(scope, styleInsertions);

			styleInsertions.SortStable((x, y) => x.Index.CompareTo(y.Index));

			int offset = 0;

			foreach (TextInsertion styleInsertion in styleInsertions) {
				textWriter.Write(HttpUtility.HtmlEncode(parsedSourceCode.Substring(offset, styleInsertion.Index - offset)));
				if (string.IsNullOrEmpty(styleInsertion.Text))
					BuildSpanForCapturedStyle(styleInsertion.Scope, textWriter);
				else
					textWriter.Write(styleInsertion.Text);
				offset = styleInsertion.Index;
			}

			textWriter.Write(HttpUtility.HtmlEncode(parsedSourceCode.Substring(offset)));
		}

		public void WriteFooter(IStyleSheet styleSheet,
								TextWriter textWriter) {
			Guard.ArgNotNull(styleSheet, "styleSheet");
			Guard.ArgNotNull(textWriter, "textWriter");

			textWriter.WriteLine();
			WriteHeaderPreEnd(textWriter);
			WriteHeaderDivEnd(textWriter);
		}

		public void WriteHeader(IStyleSheet styleSheet,
								TextWriter textWriter) {
			Guard.ArgNotNull(styleSheet, "styleSheet");
			Guard.ArgNotNull(textWriter, "textWriter");

			WriteHeaderDivStart(styleSheet, textWriter);
			WriteHeaderPreStart(textWriter);
			textWriter.WriteLine();
		}

		private static void GetStyleInsertionsForCapturedStyle(Scope scope, ICollection<TextInsertion> styleInsertions) {
			styleInsertions.Add(new TextInsertion {
				Index = scope.Index,
				Scope = scope
			});


			foreach (Scope childScope in scope.Children)
				GetStyleInsertionsForCapturedStyle(childScope, styleInsertions);

			styleInsertions.Add(new TextInsertion {
				Index = scope.Index + scope.Length,
				Text = "</span>"
			});
		}

		private static void BuildSpanForCapturedStyle(Scope scope, TextWriter writer) {
			WriteElementStart("span", scope.Name, writer);
		}

		private static void WriteHeaderDivEnd(TextWriter writer) {
			WriteElementEnd("div", writer);
		}

		private static void WriteElementEnd(string elementName,
											TextWriter writer) {
			writer.Write("</{0}>", elementName);
		}

		private static void WriteHeaderPreEnd(TextWriter writer) {
			WriteElementEnd("pre", writer);
		}

		private static void WriteHeaderPreStart(TextWriter writer) {
			WriteElementStart("pre", writer);
		}

		private static void WriteHeaderDivStart(IStyleSheet styleSheet,
												TextWriter writer) {
			WriteElementStart("div", "PlainText", writer);
		}

		private static void WriteElementStart(string elementName,
											  TextWriter writer) {
			WriteElementStart(elementName, "", writer);
		}

		private static void WriteElementStart(string elementName, string className, TextWriter writer) {
			writer.Write("<{0}", elementName);

			if (!String.IsNullOrEmpty(className)) {
				writer.Write(" class=\"");
				writer.Write(className);
				writer.Write("\"");
			}

			writer.Write(">");
		}
	}
}
