using System;
using System.Collections.Generic;

namespace EllyGGEpochFilter
{
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Main class

	public class XmlManager
	{
		private string indent = "";
		private xmlNode root;
		private xmlNode current;

		public XmlManager(string fileIndent)
		{
			this.indent = fileIndent;
		}

		public string formatXml()
		{
			if(root == null)
			{
				return "";
			}
			else
			{
				return root.ToString(0, indent);
			}
		}

		public xmlNode getXml()
		{
			return root;
		}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Adding nodes to XML
		private void addNode(xmlNode node)
		{
			if(root == null)
			{
				root = node;
				current = node;
			}
			else
			{
				//This should only happen if the root is a single tag xml
				if(current.type != NodeType.dualTagMultiLine && current.type != NodeType.multiLineComment)
				{
					throw new Exception("Error: attempt to add additional node when we are a single tag xml");
				}

				current.addNode(node);
			}
		}

		public void addSingleTag(string label, string value)
		{
			xmlNode newNode = new xmlNode(current, NodeType.singleTag, label, value);
			addNode(newNode);
		}

		public void addDualTagSingleLine(string label, string value)
		{
			xmlNode newNode = new xmlNode(current, NodeType.dualTagSingleLine, label, value);
			addNode(newNode);
		}

		public void startDualTagMultiLine(string label, string value)
		{
			xmlNode newNode = new xmlNode(current, NodeType.dualTagMultiLine, label, value);
			addNode(newNode);
			current = newNode;
		}

		public void endDualTagMultiLine()
		{
			if(current.parent == null)
			{
				current = root;
				//This line should happen when the end of the xml is reached
			}
			current = current.parent;
		}

		public void addSingleLineComment(string commentContent)
		{
			xmlNode newNode = new xmlNode(current, NodeType.singleLineComment, commentContent, "");
			addNode(newNode);
		}

		public void startMultiLineComment(string lineContent)
		{
			xmlNode newNode = new xmlNode(current, NodeType.multiLineComment, lineContent, null);
			addNode(newNode);
			current = newNode;
		}

		public void addLineToMultiLineComment(string lineContent)
		{
			if(current.type != NodeType.multiLineComment)
			{
				throw new Exception("Error: attempt to add line to a multi line comment but we are actually in a \"" + current.type.ToString() + "\"");
			}
			xmlNode newNode = new xmlNode(current, NodeType.commentLine, lineContent, "");
			addNode(newNode);
		}

		public void endMultiLineComment(string lineContent)
		{
			if(current.type != NodeType.multiLineComment)
			{
				throw new Exception("Error: attempt to close a multi line comment but we are actually in a \"" + current.type.ToString() + "\"");
			}
			current.value = lineContent;
			endDualTagMultiLine();
		}

// Adding nodes to XML
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Turning file contents into XML

		private void process(string contents)
		{
			string currentLine;
			int lineBreakLocation = contents.IndexOf('\n');
			if(lineBreakLocation == -1)
			{
				currentLine = contents;
				contents = null;
			}
			else
			{
				currentLine = contents.Substring(0, lineBreakLocation);
				contents = contents.Substring(lineBreakLocation + 1);
			}


			if(currentLine.Contains("<!--") && currentLine.Contains("-->"))
			{
				//This is a single line comment
				int start = currentLine.IndexOf("<!--") + 4;
				int end = currentLine.IndexOf("-->");

				this.addSingleLineComment(currentLine.Substring(start, end - start));

			}
			else if(currentLine.Contains("<!--"))
			{
				//This is the start of a multi line comment
				int start = currentLine.IndexOf("<!--") + 4;

				this.startMultiLineComment(currentLine.Substring(start));
			}
			else if(currentLine.Contains("-->"))
			{
				//This is the end of a multi line comment
				int end = currentLine.IndexOf("-->");

				this.endMultiLineComment(currentLine.Substring(0, end));
			}
			else if(currentLine.Contains("<") && currentLine.Contains("/>"))
			{
				//This line is a single tag
				int start = currentLine.IndexOf("<") + 1;
				currentLine = currentLine.Substring(start);
				start = 0;
				
				int space = currentLine.IndexOf(" ");
				int end = currentLine.IndexOf("/>");

				string label;
				string value = null;
				if(space == -1 || space >= end - 1)
				{
					label = currentLine.Substring(start, end - start);
				}
				else
				{
					label = currentLine.Substring(start, space - start);
					value = currentLine.Substring(space + 1, end - space - 1);
				}

				this.addSingleTag(label, value);
			}
			else if(currentLine.Contains(">")
				&& currentLine.Contains("</")
				&& currentLine.IndexOf(">") < currentLine.IndexOf("</"))
			{
				//This is a single line dual tag
				int start = currentLine.IndexOf("<") + 1;
				currentLine = currentLine.Substring(start);
				start = 0;

				int end = currentLine.IndexOf(">");

				string label = currentLine.Substring(start, end - start);
				currentLine = currentLine.Substring(end + 1);
				start = 0;

				end = currentLine.IndexOf("</");
				string value = currentLine.Substring(start, end - start);

				this.addDualTagSingleLine(label, value);
			}
			else if(currentLine.Contains("</"))
			{
				//This is the end of a multi line tag
				this.endDualTagMultiLine();
			}
			else if(currentLine.Contains("<"))
			{
				//This is the start of a multi line tag
				int start = currentLine.IndexOf("<") + 1;
				currentLine = currentLine.Substring(start);
				start = 0;

				int space = currentLine.IndexOf(" ");
				int end = currentLine.IndexOf(">");

				string label;
				string value = null;
				if(space == -1 || space >= end - 1)
				{
					label = currentLine.Substring(start, end - start);
				}
				else
				{
					label = currentLine.Substring(start, space - start);
					value = currentLine.Substring(space + 1, end - space - 1);
				}

				this.startDualTagMultiLine(label, value);
			}
			else
			{
				//This is a multi line comment
				this.addLineToMultiLineComment(currentLine);
			}

			if(contents != null)
			{
				process(contents);
			}
		}

		public void loadXmlFromFileContents(string contents)
		{
			//Clear the tree to start a new one
			root = null;

			process(contents);

			//Move to root of xml structure
			current = root;
		}

// Turning file contents into XML
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	}

// Main class
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Data structures

	public enum NodeType {singleTag, dualTagSingleLine, dualTagMultiLine, singleLineComment, multiLineComment, commentLine}

	public class xmlNode
	{
		public NodeType type;

		public string label;
		public string value;

		public xmlNode parent;
		public List<xmlNode> children;

		public xmlNode(xmlNode parent, NodeType nodeType, string nodeLabel, string nodeValue)
		{
			this.parent = parent;

			this.type = nodeType;
			
			this.label = nodeLabel;
			
			this.value = nodeValue;
			if(this.value == null)
			{
				this.value = "";
			}

			if(nodeType == NodeType.dualTagMultiLine || nodeType == NodeType.multiLineComment)
			{
				children = new List<xmlNode>();
			}
		}

		public void addNode(xmlNode subNode)
		{
			if(this.type == NodeType.dualTagMultiLine || this.type == NodeType.multiLineComment)
			{
				children.Add(subNode);
			}
			else
			{
				throw new Exception("Error: attempt to add child node to an invalid NodeType");
			}
		}

		public string ToString(int depth, string indent)
		{
			string result = "";

			string padding = "";
			for(int i = 0; i < depth; i++)
			{
				padding += indent;
			}

			if(this.type == NodeType.singleTag)
			{
				result = padding + "<" + this.label + " " + this.value + "/>";
			}
			else if(this.type == NodeType.dualTagSingleLine)
			{
				result = padding
					+ "<" + this.label + ">"
					+ this.value
					+ "</" + this.label + ">";
			}
			else if(this.type == NodeType.dualTagMultiLine)
			{
				result = padding + "<" + this.label
					+ (this.value == "" ? "" : " " + this.value)
					+ ">"
					+ "\n";
				foreach(xmlNode child in this.children)
				{
					result += child.ToString(depth + 1, indent) + "\n";
				}
				result += padding + "</" + this.label + ">";
			}
			else if(this.type == NodeType.singleLineComment)
			{
				result = padding + "<!--" + this.label + this.value + "-->";
			}
			else if(this.type == NodeType.multiLineComment)
			{
				result = padding + "<!--" + this.label + "\n";
				foreach(xmlNode child in this.children)
				{
					result += child.ToString(0, "") + "\n";
				}
				result += this.value + "-->";
			}
			else if(this.type == NodeType.commentLine)
			{
				result = this.label + this.value;
			}
			else
			{
				throw new Exception("Error, encountered new NodeType \"" + this.type.ToString() + "\" which does not have a defined string formatting");
			}

			return result;
		}
	}

// Data structures
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
}