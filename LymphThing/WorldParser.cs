//Generated with Imperator Parser Generator v. 2011-01-17
//Imperator by Max 'Shrinker' Wieden
//http://shrinker.beyond-veils.de/
//http://shrinker.scottbaker.eu/
using System;
using System.Collections.Generic;

namespace Ponykart.IO.WorldParser {
	///<summary/>
	public class ParserException : Exception {
		///<summary/><param name="message"/>
		public ParserException(string message) :
			base(message) {
		}
	}

	///<summary/>
	public enum NodeType {
		///<summary/>
		Tok_EOF,
		///<summary/>
		Tok_Name,
		///<summary/>
		Tok_KeyActor,
		///<summary/>
		Tok_KeyDestructible,
		///<summary/>
		Tok_KeyEntities,
		///<summary/>
		Tok_KeyFalse,
		///<summary/>
		Tok_KeyFlags,
		///<summary/>
		Tok_KeyNumbers,
		///<summary/>
		Tok_KeyOverrides,
		///<summary/>
		Tok_KeyTrue,
		///<summary/>
		Tok_FloatLiteral,
		///<summary/>
		Tok_StringLiteral,
		///<summary/>
		Tok_Assign,
		///<summary/>
		Tok_Colon,
		///<summary/>
		Tok_Comma,
		///<summary/>
		Tok_LBrace,
		///<summary/>
		Tok_LPar,
		///<summary/>
		Tok_RBrace,
		///<summary/>
		Tok_RPar,
		///<summary/>
		Tok_SingleLineComment,
		///<summary/>
		Tok_MultiLineComment,
		///<summary/>
		Tok_Whitespace,
		///<summary/>
		Rule_Start,
		///<summary/>
		Rule_AnyName,
		///<summary/>
		Rule_FlagsSection,
		///<summary/>
		Rule_NumbersSection,
		///<summary/>
		Rule_EntitiesSection,
		///<summary/>
		Rule_FlagEntry,
		///<summary/>
		Rule_NumberEntry,
		///<summary/>
		Rule_EntityDef,
		///<summary/>
		Rule_EntityProperty,
		///<summary/>
		Rule_EPString,
		///<summary/>
		Rule_EPVector3D,
		///<summary/>
		Rule_EPFloat,
		///<summary/>
		Rule_EPBool,
		///<summary/>
		Rule_OverridesBlock
	}

	///<summary/>
	public abstract class Node {
		///<summary/><param name="type"/><returns/>
		public static string TypeName(NodeType type) {
			string s = type.ToString();
			return s.Substring(s.IndexOf('_') + 1);
		}

		///<summary/>
		public readonly NodeType Type;

		///<summary/><param name="type"/>
		public Node(NodeType type) {
			Type = type;
		}
	}

	///<summary>represents an inner node</summary>
	public class RuleInstance : Node {
		///<summary/>
		public readonly Node[] Children;

		///<summary/><param name="type"/><param name="children"/>
		public RuleInstance(NodeType type, Node[] children) :
			base(type) {
			Children = children;
		}
	}

	///<summary>represents a leaf node</summary>
	public class Token : Node {
		///<summary/>
		public readonly Token[] PrecedingFillerTokens;
		///<summary/>
		public readonly string Image;
		///<summary/>
		public readonly int LineNr;
		///<summary/>
		public readonly int CharNr;

		private static Dictionary<string, NodeType> specForTok_Name;

		static Token() {
			specForTok_Name = new Dictionary<string, NodeType>();
			specForTok_Name.Add("Actor", NodeType.Tok_KeyActor);
			specForTok_Name.Add("Destructible", NodeType.Tok_KeyDestructible);
			specForTok_Name.Add("Entities", NodeType.Tok_KeyEntities);
			specForTok_Name.Add("false", NodeType.Tok_KeyFalse);
			specForTok_Name.Add("Flags", NodeType.Tok_KeyFlags);
			specForTok_Name.Add("Numbers", NodeType.Tok_KeyNumbers);
			specForTok_Name.Add("Overrides", NodeType.Tok_KeyOverrides);
			specForTok_Name.Add("true", NodeType.Tok_KeyTrue);
		}

		private static NodeType specializeType(NodeType type, string image) {
			NodeType type2;
			switch (type) {
				case NodeType.Tok_Name:
					if (specForTok_Name.TryGetValue(image, out type2))
						return type2;
					break;
			}
			return type;
		}

		///<summary/><param name="precedingFillerTokens"/><param name="type"/><param name="image"/><param name="lineNr"/><param name="charNr"/>
		public Token(Token[] precedingFillerTokens, NodeType type, string image, int lineNr, int charNr) :
			base(specializeType(type, image)) {
			PrecedingFillerTokens = precedingFillerTokens;
			Image = image;
			LineNr = lineNr;
			CharNr = charNr;
		}
	}

	///<summary/>
	public class Parser {
		private string source;
		private int index, length, currLine, currChar, laOffset;
		private readonly List <Token> fetchedTokens;
		private readonly Stack<bool > laSuccess;
		private readonly Stack<int  > laOffsets;
		private readonly Stack<bool > onceOrMoreB;

		///<summary/>
		public Parser() {
			fetchedTokens = new List<Token>();
			laSuccess = new Stack<bool>();
			laOffsets = new Stack<int>();
			onceOrMoreB = new Stack<bool>();
		}

		///<summary/><param name="source"/><returns/>
		public RuleInstance Parse(string source) {
			this.source = source;
			index = 0;
			length = source.Length;
			laOffset = 0;
			currLine = 1;
			currChar = 1;
			RuleInstance r;
			try {
				r = matchStart();
			}
			finally {
				this.source = null;
				fetchedTokens.Clear();
				laSuccess.Clear();
				laOffsets.Clear();
				onceOrMoreB.Clear();
			}
			return r;
		}

		private Token nextToken() {
			return nextToken(true);
		}

		private Token fetchToken(int offset) {
			while (fetchedTokens.Count <= offset) {
				Token tok = nextToken(false);
				fetchedTokens.Add(tok);
				//if token stream is exhausted, return EOF
				if (tok.Type == NodeType.Tok_EOF) {
					offset = fetchedTokens.Count - 1;
					break;
				}
			}
			return fetchedTokens[offset];
		}

		private Token[] getFillerTokens() {
			List<Token> result = new List<Token>();
			Stack<int> indices = new Stack<int>(), currLines = new Stack<int>(), currChars = new Stack<int>();
			char c;
			bool pass;
			while (true) {
				int oldIndex = index, lastLine = currLine, lastChar = currChar;

				//Token "SingleLineComment"
				pass = true;
				indices.Push(index);
				currLines.Push(currLine);
				currChars.Push(currChar);
				if (index + 2 < length && source.Substring(index, 2).Equals("//")) {
					index += 2;
					currChar += 2;
				}
				else
					pass = false;
				if (pass) {
					while (true) {
						indices.Push(index);
						currLines.Push(currLine);
						currChars.Push(currChar);
						if (index < length && (c = source[index]) != '\r' && c != '\n') {
							index++;
							currChar++;
						}
						else
							pass = false;
						if (pass) {
							indices.Pop();
							currLines.Pop();
							currChars.Pop();
						}
						else {
							pass = true;
							index = indices.Pop();
							currLine = currLines.Pop();
							currChar = currChars.Pop();
							break;
						}
					}
					if (pass) {
						indices.Push(index);
						currLines.Push(currLine);
						currChars.Push(currChar);
						indices.Push(index);
						currLines.Push(currLine);
						currChars.Push(currChar);
						if (index + 2 < length && source.Substring(index, 2).Equals("\r\n")) {
							index += 2;
							currLine++;
							currChar = 1;
						}
						else
							pass = false;
						if (!pass) {
							pass = true;
							index = indices.Peek();
							currLine = currLines.Peek();
							currChar = currChars.Peek();
							if (index < length && ((c = source[index]) == '\r' || c == '\n')) {
								index++;
								if (c == '\r') {
								}
								else
									if (c == '\n') {
										currLine++;
										currChar = 1;
									}
									else
										currChar++;
							}
							else
								pass = false;
						}
						indices.Pop();
						currLines.Pop();
						currChars.Pop();
						if (pass) {
							indices.Pop();
							currLines.Pop();
							currChars.Pop();
						}
						else {
							pass = true;
							index = indices.Pop();
							currLine = currLines.Pop();
							currChar = currChars.Pop();
						}
					}
				}
				if (pass) {
					indices.Pop();
					currLines.Pop();
					currChars.Pop();
					result.Add(new Token(null, NodeType.Tok_SingleLineComment, source.Substring(oldIndex, index - oldIndex), lastLine, lastChar));
					continue;
				}
				else {
					index = indices.Pop();
					currLine = currLines.Pop();
					currChar = currChars.Pop();
				}

				//Token "MultiLineComment"
				pass = true;
				indices.Push(index);
				currLines.Push(currLine);
				currChars.Push(currChar);
				if (index + 2 < length && source.Substring(index, 2).Equals("/*")) {
					index += 2;
					currChar += 2;
				}
				else
					pass = false;
				if (pass) {
					while (true) {
						indices.Push(index);
						currLines.Push(currLine);
						currChars.Push(currChar);
						if (index < length && (c = source[index]) != '*') {
							index++;
							if (c == '\r') {
							}
							else
								if (c == '\n') {
									currLine++;
									currChar = 1;
								}
								else
									currChar++;
						}
						else
							pass = false;
						if (pass) {
							indices.Pop();
							currLines.Pop();
							currChars.Pop();
						}
						else {
							pass = true;
							index = indices.Pop();
							currLine = currLines.Pop();
							currChar = currChars.Pop();
							break;
						}
					}
					if (pass) {
						indices.Push(index);
						currLines.Push(currLine);
						currChars.Push(currChar);
						onceOrMoreB.Push(false);
						while (true) {
							indices.Push(index);
							currLines.Push(currLine);
							currChars.Push(currChar);
							if (index < length && source[index] == '*') {
								index++;
								currChar++;
							}
							else
								pass = false;
							if (pass) {
								onceOrMoreB.Pop();
								onceOrMoreB.Push(true);
								indices.Pop();
								currLines.Pop();
								currChars.Pop();
							}
							else {
								index = indices.Pop();
								currLine = currLines.Pop();
								currChar = currChars.Pop();
								break;
							}
						}
						pass = onceOrMoreB.Pop();
						if (pass) {
							indices.Pop();
							currLines.Pop();
							currChars.Pop();
						}
						else {
							index = indices.Pop();
							currLine = currLines.Pop();
							currChar = currChars.Pop();
						}
						if (pass) {
							while (true) {
								indices.Push(index);
								currLines.Push(currLine);
								currChars.Push(currChar);
								if (index < length && (c = source[index]) != '*' && c != '/') {
									index++;
									if (c == '\r') {
									}
									else
										if (c == '\n') {
											currLine++;
											currChar = 1;
										}
										else
											currChar++;
								}
								else
									pass = false;
								if (pass) {
									while (true) {
										indices.Push(index);
										currLines.Push(currLine);
										currChars.Push(currChar);
										if (index < length && (c = source[index]) != '*') {
											index++;
											if (c == '\r') {
											}
											else
												if (c == '\n') {
													currLine++;
													currChar = 1;
												}
												else
													currChar++;
										}
										else
											pass = false;
										if (pass) {
											indices.Pop();
											currLines.Pop();
											currChars.Pop();
										}
										else {
											pass = true;
											index = indices.Pop();
											currLine = currLines.Pop();
											currChar = currChars.Pop();
											break;
										}
									}
									if (pass) {
										indices.Push(index);
										currLines.Push(currLine);
										currChars.Push(currChar);
										onceOrMoreB.Push(false);
										while (true) {
											indices.Push(index);
											currLines.Push(currLine);
											currChars.Push(currChar);
											if (index < length && source[index] == '*') {
												index++;
												currChar++;
											}
											else
												pass = false;
											if (pass) {
												onceOrMoreB.Pop();
												onceOrMoreB.Push(true);
												indices.Pop();
												currLines.Pop();
												currChars.Pop();
											}
											else {
												index = indices.Pop();
												currLine = currLines.Pop();
												currChar = currChars.Pop();
												break;
											}
										}
										pass = onceOrMoreB.Pop();
										if (pass) {
											indices.Pop();
											currLines.Pop();
											currChars.Pop();
										}
										else {
											index = indices.Pop();
											currLine = currLines.Pop();
											currChar = currChars.Pop();
										}
									}
								}
								if (pass) {
									indices.Pop();
									currLines.Pop();
									currChars.Pop();
								}
								else {
									pass = true;
									index = indices.Pop();
									currLine = currLines.Pop();
									currChar = currChars.Pop();
									break;
								}
							}
							if (pass) {
								if (index < length && source[index] == '/') {
									index++;
									currChar++;
								}
								else
									pass = false;
							}
						}
					}
				}
				if (pass) {
					indices.Pop();
					currLines.Pop();
					currChars.Pop();
					result.Add(new Token(null, NodeType.Tok_MultiLineComment, source.Substring(oldIndex, index - oldIndex), lastLine, lastChar));
					continue;
				}
				else {
					index = indices.Pop();
					currLine = currLines.Pop();
					currChar = currChars.Pop();
				}

				//Token "Whitespace"
				pass = true;
				indices.Push(index);
				currLines.Push(currLine);
				currChars.Push(currChar);
				indices.Push(index);
				currLines.Push(currLine);
				currChars.Push(currChar);
				onceOrMoreB.Push(false);
				while (true) {
					indices.Push(index);
					currLines.Push(currLine);
					currChars.Push(currChar);
					if (index < length && ((c = source[index]) == '\t' || c == ' ' || c == '\r' || c == '\n')) {
						index++;
						if (c == '\r') {
						}
						else
							if (c == '\n') {
								currLine++;
								currChar = 1;
							}
							else
								currChar++;
					}
					else
						pass = false;
					if (pass) {
						onceOrMoreB.Pop();
						onceOrMoreB.Push(true);
						indices.Pop();
						currLines.Pop();
						currChars.Pop();
					}
					else {
						index = indices.Pop();
						currLine = currLines.Pop();
						currChar = currChars.Pop();
						break;
					}
				}
				pass = onceOrMoreB.Pop();
				if (pass) {
					indices.Pop();
					currLines.Pop();
					currChars.Pop();
				}
				else {
					index = indices.Pop();
					currLine = currLines.Pop();
					currChar = currChars.Pop();
				}
				if (pass) {
					indices.Pop();
					currLines.Pop();
					currChars.Pop();
					result.Add(new Token(null, NodeType.Tok_Whitespace, source.Substring(oldIndex, index - oldIndex), lastLine, lastChar));
					continue;
				}
				else {
					index = indices.Pop();
					currLine = currLines.Pop();
					currChar = currChars.Pop();
				}

				break;
			}

			return result.ToArray();
		}

		private Token nextToken(bool useFetched) {
			if (useFetched && fetchedTokens.Count != 0) {
				Token tok = fetchedTokens[0];
				fetchedTokens.RemoveAt(0);
				return tok;
			}

			Token[] fillers = getFillerTokens();

			if (index == length)
				return new Token(fillers, NodeType.Tok_EOF, "", currLine, currChar);

			Stack<int> indices = new Stack<int>(), currLines = new Stack<int>(), currChars = new Stack<int>();
			char c;
			bool pass;
			int oldIndex = index, lastLine = currLine, lastChar = currChar;

			//Token "Name"
			pass = true;
			indices.Push(index);
			currLines.Push(currLine);
			currChars.Push(currChar);
			if (index < length && (c = source[index]) >= 'A' && c <= 'Z') {
				index++;
				currChar++;
			}
			else
				pass = false;
			if (!pass) {
				pass = true;
				if (index < length && (c = source[index]) >= 'a' && c <= 'z') {
					index++;
					currChar++;
				}
				else
					pass = false;
				if (!pass) {
					pass = true;
					if (index < length && ((c = source[index]) == '_' || c == '$')) {
						index++;
						currChar++;
					}
					else
						pass = false;
				}
			}
			if (pass) {
				while (true) {
					indices.Push(index);
					currLines.Push(currLine);
					currChars.Push(currChar);
					indices.Push(index);
					currLines.Push(currLine);
					currChars.Push(currChar);
					if (index < length && (c = source[index]) >= 'A' && c <= 'Z') {
						index++;
						currChar++;
					}
					else
						pass = false;
					if (!pass) {
						pass = true;
						if (index < length && (c = source[index]) >= 'a' && c <= 'z') {
							index++;
							currChar++;
						}
						else
							pass = false;
						if (!pass) {
							pass = true;
							if (index < length && ((c = source[index]) == '_' || c == '$')) {
								index++;
								currChar++;
							}
							else
								pass = false;
						}
					}
					if (!pass) {
						pass = true;
						index = indices.Peek();
						currLine = currLines.Peek();
						currChar = currChars.Peek();
						if (index < length && (c = source[index]) >= '0' && c <= '9') {
							index++;
							currChar++;
						}
						else
							pass = false;
					}
					indices.Pop();
					currLines.Pop();
					currChars.Pop();
					if (pass) {
						indices.Pop();
						currLines.Pop();
						currChars.Pop();
					}
					else {
						pass = true;
						index = indices.Pop();
						currLine = currLines.Pop();
						currChar = currChars.Pop();
						break;
					}
				}
			}
			if (pass) {
				indices.Pop();
				currLines.Pop();
				currChars.Pop();
				return new Token(fillers, NodeType.Tok_Name, source.Substring(oldIndex, index - oldIndex), lastLine, lastChar);
			}
			else {
				index = indices.Pop();
				currLine = currLines.Pop();
				currChar = currChars.Pop();
			}

			//Token "FloatLiteral"
			pass = true;
			indices.Push(index);
			currLines.Push(currLine);
			currChars.Push(currChar);
			indices.Push(index);
			currLines.Push(currLine);
			currChars.Push(currChar);
			indices.Push(index);
			currLines.Push(currLine);
			currChars.Push(currChar);
			if (index < length && source[index] == '-') {
				index++;
				currChar++;
			}
			else
				pass = false;
			if (!pass) {
				pass = true;
				index = indices.Peek();
				currLine = currLines.Peek();
				currChar = currChars.Peek();
				if (index < length && source[index] == '+') {
					index++;
					currChar++;
				}
				else
					pass = false;
			}
			indices.Pop();
			currLines.Pop();
			currChars.Pop();
			if (pass) {
				indices.Pop();
				currLines.Pop();
				currChars.Pop();
			}
			else {
				pass = true;
				index = indices.Pop();
				currLine = currLines.Pop();
				currChar = currChars.Pop();
			}
			if (pass) {
				indices.Push(index);
				currLines.Push(currLine);
				currChars.Push(currChar);
				indices.Push(index);
				currLines.Push(currLine);
				currChars.Push(currChar);
				onceOrMoreB.Push(false);
				while (true) {
					indices.Push(index);
					currLines.Push(currLine);
					currChars.Push(currChar);
					if (index < length && (c = source[index]) >= '0' && c <= '9') {
						index++;
						currChar++;
					}
					else
						pass = false;
					if (pass) {
						onceOrMoreB.Pop();
						onceOrMoreB.Push(true);
						indices.Pop();
						currLines.Pop();
						currChars.Pop();
					}
					else {
						index = indices.Pop();
						currLine = currLines.Pop();
						currChar = currChars.Pop();
						break;
					}
				}
				pass = onceOrMoreB.Pop();
				if (pass) {
					indices.Pop();
					currLines.Pop();
					currChars.Pop();
				}
				else {
					index = indices.Pop();
					currLine = currLines.Pop();
					currChar = currChars.Pop();
				}
				if (pass) {
					if (index < length && source[index] == '.') {
						index++;
						currChar++;
					}
					else
						pass = false;
					if (pass) {
						while (true) {
							indices.Push(index);
							currLines.Push(currLine);
							currChars.Push(currChar);
							if (index < length && (c = source[index]) >= '0' && c <= '9') {
								index++;
								currChar++;
							}
							else
								pass = false;
							if (pass) {
								indices.Pop();
								currLines.Pop();
								currChars.Pop();
							}
							else {
								pass = true;
								index = indices.Pop();
								currLine = currLines.Pop();
								currChar = currChars.Pop();
								break;
							}
						}
					}
				}
				if (!pass) {
					pass = true;
					index = indices.Peek();
					currLine = currLines.Peek();
					currChar = currChars.Peek();
					indices.Push(index);
					currLines.Push(currLine);
					currChars.Push(currChar);
					if (index < length && source[index] == '.') {
						index++;
						currChar++;
					}
					else
						pass = false;
					if (pass) {
						indices.Pop();
						currLines.Pop();
						currChars.Pop();
					}
					else {
						pass = true;
						index = indices.Pop();
						currLine = currLines.Pop();
						currChar = currChars.Pop();
					}
					if (pass) {
						indices.Push(index);
						currLines.Push(currLine);
						currChars.Push(currChar);
						onceOrMoreB.Push(false);
						while (true) {
							indices.Push(index);
							currLines.Push(currLine);
							currChars.Push(currChar);
							if (index < length && (c = source[index]) >= '0' && c <= '9') {
								index++;
								currChar++;
							}
							else
								pass = false;
							if (pass) {
								onceOrMoreB.Pop();
								onceOrMoreB.Push(true);
								indices.Pop();
								currLines.Pop();
								currChars.Pop();
							}
							else {
								index = indices.Pop();
								currLine = currLines.Pop();
								currChar = currChars.Pop();
								break;
							}
						}
						pass = onceOrMoreB.Pop();
						if (pass) {
							indices.Pop();
							currLines.Pop();
							currChars.Pop();
						}
						else {
							index = indices.Pop();
							currLine = currLines.Pop();
							currChar = currChars.Pop();
						}
					}
				}
				indices.Pop();
				currLines.Pop();
				currChars.Pop();
				if (pass) {
					indices.Push(index);
					currLines.Push(currLine);
					currChars.Push(currChar);
					if (index < length && ((c = source[index]) == 'E' || c == 'e')) {
						index++;
						currChar++;
					}
					else
						pass = false;
					if (pass) {
						if (index < length && ((c = source[index]) == '+' || c == '-')) {
							index++;
							currChar++;
						}
						else
							pass = false;
						if (pass) {
							indices.Push(index);
							currLines.Push(currLine);
							currChars.Push(currChar);
							onceOrMoreB.Push(false);
							while (true) {
								indices.Push(index);
								currLines.Push(currLine);
								currChars.Push(currChar);
								if (index < length && (c = source[index]) >= '0' && c <= '9') {
									index++;
									currChar++;
								}
								else
									pass = false;
								if (pass) {
									onceOrMoreB.Pop();
									onceOrMoreB.Push(true);
									indices.Pop();
									currLines.Pop();
									currChars.Pop();
								}
								else {
									index = indices.Pop();
									currLine = currLines.Pop();
									currChar = currChars.Pop();
									break;
								}
							}
							pass = onceOrMoreB.Pop();
							if (pass) {
								indices.Pop();
								currLines.Pop();
								currChars.Pop();
							}
							else {
								index = indices.Pop();
								currLine = currLines.Pop();
								currChar = currChars.Pop();
							}
						}
					}
					if (pass) {
						indices.Pop();
						currLines.Pop();
						currChars.Pop();
					}
					else {
						pass = true;
						index = indices.Pop();
						currLine = currLines.Pop();
						currChar = currChars.Pop();
					}
				}
			}
			if (pass) {
				indices.Pop();
				currLines.Pop();
				currChars.Pop();
				return new Token(fillers, NodeType.Tok_FloatLiteral, source.Substring(oldIndex, index - oldIndex), lastLine, lastChar);
			}
			else {
				index = indices.Pop();
				currLine = currLines.Pop();
				currChar = currChars.Pop();
			}

			//Token "StringLiteral"
			pass = true;
			indices.Push(index);
			currLines.Push(currLine);
			currChars.Push(currChar);
			if (index < length && source[index] == '\"') {
				index++;
				currChar++;
			}
			else
				pass = false;
			if (pass) {
				while (true) {
					indices.Push(index);
					currLines.Push(currLine);
					currChars.Push(currChar);
					indices.Push(index);
					currLines.Push(currLine);
					currChars.Push(currChar);
					if (index < length && source[index] == '\\') {
						index++;
						currChar++;
					}
					else
						pass = false;
					if (pass) {
						if (index < length && ((c = source[index]) == '\"' || c == '\\')) {
							index++;
							currChar++;
						}
						else
							pass = false;
					}
					if (!pass) {
						pass = true;
						index = indices.Peek();
						currLine = currLines.Peek();
						currChar = currChars.Peek();
						if (index < length && (c = source[index]) != '\"' && c != '\\') {
							index++;
							if (c == '\r') {
							}
							else
								if (c == '\n') {
									currLine++;
									currChar = 1;
								}
								else
									currChar++;
						}
						else
							pass = false;
					}
					indices.Pop();
					currLines.Pop();
					currChars.Pop();
					if (pass) {
						indices.Pop();
						currLines.Pop();
						currChars.Pop();
					}
					else {
						pass = true;
						index = indices.Pop();
						currLine = currLines.Pop();
						currChar = currChars.Pop();
						break;
					}
				}
				if (pass) {
					if (index < length && source[index] == '\"') {
						index++;
						currChar++;
					}
					else
						pass = false;
				}
			}
			if (pass) {
				indices.Pop();
				currLines.Pop();
				currChars.Pop();
				return new Token(fillers, NodeType.Tok_StringLiteral, source.Substring(oldIndex, index - oldIndex), lastLine, lastChar);
			}
			else {
				index = indices.Pop();
				currLine = currLines.Pop();
				currChar = currChars.Pop();
			}

			//Token "Assign"
			pass = true;
			if (index < length && source[index] == '=') {
				index++;
				currChar++;
			}
			else
				pass = false;
			if (pass)
				return new Token(fillers, NodeType.Tok_Assign, source.Substring(oldIndex, index - oldIndex), lastLine, lastChar);

			//Token "Colon"
			pass = true;
			if (index < length && source[index] == ':') {
				index++;
				currChar++;
			}
			else
				pass = false;
			if (pass)
				return new Token(fillers, NodeType.Tok_Colon, source.Substring(oldIndex, index - oldIndex), lastLine, lastChar);

			//Token "Comma"
			pass = true;
			if (index < length && source[index] == ',') {
				index++;
				currChar++;
			}
			else
				pass = false;
			if (pass)
				return new Token(fillers, NodeType.Tok_Comma, source.Substring(oldIndex, index - oldIndex), lastLine, lastChar);

			//Token "LBrace"
			pass = true;
			if (index < length && source[index] == '{') {
				index++;
				currChar++;
			}
			else
				pass = false;
			if (pass)
				return new Token(fillers, NodeType.Tok_LBrace, source.Substring(oldIndex, index - oldIndex), lastLine, lastChar);

			//Token "LPar"
			pass = true;
			if (index < length && source[index] == '(') {
				index++;
				currChar++;
			}
			else
				pass = false;
			if (pass)
				return new Token(fillers, NodeType.Tok_LPar, source.Substring(oldIndex, index - oldIndex), lastLine, lastChar);

			//Token "RBrace"
			pass = true;
			if (index < length && source[index] == '}') {
				index++;
				currChar++;
			}
			else
				pass = false;
			if (pass)
				return new Token(fillers, NodeType.Tok_RBrace, source.Substring(oldIndex, index - oldIndex), lastLine, lastChar);

			//Token "RPar"
			pass = true;
			if (index < length && source[index] == ')') {
				index++;
				currChar++;
			}
			else
				pass = false;
			if (pass)
				return new Token(fillers, NodeType.Tok_RPar, source.Substring(oldIndex, index - oldIndex), lastLine, lastChar);

			throw new ParserException("Line " + currLine + ", Char " + currChar + ": No token match");
		}

		private RuleInstance matchStart() {
			List<Node> nodes = new List<Node>();
			Token tok;

			nodes.Add(matchFlagsSection());
			nodes.Add(matchNumbersSection());
			nodes.Add(matchEntitiesSection());
			if ((tok = nextToken()).Type != NodeType.Tok_EOF)
				throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected EOF token");
			nodes.Add(tok);


			return new RuleInstance(NodeType.Rule_Start, nodes.ToArray());
		}

		private RuleInstance matchAnyName() {
			List<Node> nodes = new List<Node>();
			Token tok;

			if (fetchToken(laOffset).Type == NodeType.Tok_Name) {
				if ((tok = nextToken()).Type != NodeType.Tok_Name)
					throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected Name token");
				nodes.Add(tok);
			}
			else {
				if (fetchToken(laOffset).Type == NodeType.Tok_KeyActor) {
					if ((tok = nextToken()).Type != NodeType.Tok_KeyActor)
						throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected KeyActor token");
					nodes.Add(tok);
				}
				else {
					if (fetchToken(laOffset).Type == NodeType.Tok_KeyDestructible) {
						if ((tok = nextToken()).Type != NodeType.Tok_KeyDestructible)
							throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected KeyDestructible token");
						nodes.Add(tok);
					}
					else {
						if (fetchToken(laOffset).Type == NodeType.Tok_KeyEntities) {
							if ((tok = nextToken()).Type != NodeType.Tok_KeyEntities)
								throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected KeyEntities token");
							nodes.Add(tok);
						}
						else {
							if (fetchToken(laOffset).Type == NodeType.Tok_KeyFalse) {
								if ((tok = nextToken()).Type != NodeType.Tok_KeyFalse)
									throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected KeyFalse token");
								nodes.Add(tok);
							}
							else {
								if (fetchToken(laOffset).Type == NodeType.Tok_KeyFlags) {
									if ((tok = nextToken()).Type != NodeType.Tok_KeyFlags)
										throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected KeyFlags token");
									nodes.Add(tok);
								}
								else {
									if (fetchToken(laOffset).Type == NodeType.Tok_KeyNumbers) {
										if ((tok = nextToken()).Type != NodeType.Tok_KeyNumbers)
											throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected KeyNumbers token");
										nodes.Add(tok);
									}
									else {
										if (fetchToken(laOffset).Type == NodeType.Tok_KeyOverrides) {
											if ((tok = nextToken()).Type != NodeType.Tok_KeyOverrides)
												throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected KeyOverrides token");
											nodes.Add(tok);
										}
										else {
											if ((tok = nextToken()).Type != NodeType.Tok_KeyTrue)
												throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected KeyTrue token");
											nodes.Add(tok);
										}
									}
								}
							}
						}
					}
				}
			}


			return new RuleInstance(NodeType.Rule_AnyName, nodes.ToArray());
		}

		private RuleInstance matchFlagsSection() {
			List<Node> nodes = new List<Node>();
			Token tok;

			if ((tok = nextToken()).Type != NodeType.Tok_KeyFlags)
				throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected KeyFlags token");
			nodes.Add(tok);
			if ((tok = nextToken()).Type != NodeType.Tok_Colon)
				throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected Colon token");
			nodes.Add(tok);
			while (true) {
				laOffsets.Push(laOffset);
				laSuccess.Push(true);
				lookaheadAnyName();
				if (laSuccess.Peek()) {
					if (fetchToken(laOffset).Type == NodeType.Tok_Assign)
						laOffset++;
					else {
						laSuccess.Pop();
						laSuccess.Push(false);
					}
				}
				laOffset = laOffsets.Pop();
				if (laSuccess.Pop()) {
					nodes.Add(matchFlagEntry());
				}
				else
					break;
			}


			return new RuleInstance(NodeType.Rule_FlagsSection, nodes.ToArray());
		}

		private RuleInstance matchNumbersSection() {
			List<Node> nodes = new List<Node>();
			Token tok;

			if ((tok = nextToken()).Type != NodeType.Tok_KeyNumbers)
				throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected KeyNumbers token");
			nodes.Add(tok);
			if ((tok = nextToken()).Type != NodeType.Tok_Colon)
				throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected Colon token");
			nodes.Add(tok);
			while (true) {
				laOffsets.Push(laOffset);
				laSuccess.Push(true);
				lookaheadAnyName();
				if (laSuccess.Peek()) {
					if (fetchToken(laOffset).Type == NodeType.Tok_Assign)
						laOffset++;
					else {
						laSuccess.Pop();
						laSuccess.Push(false);
					}
				}
				laOffset = laOffsets.Pop();
				if (laSuccess.Pop()) {
					nodes.Add(matchNumberEntry());
				}
				else
					break;
			}


			return new RuleInstance(NodeType.Rule_NumbersSection, nodes.ToArray());
		}

		private RuleInstance matchEntitiesSection() {
			List<Node> nodes = new List<Node>();
			Token tok;

			if ((tok = nextToken()).Type != NodeType.Tok_KeyEntities)
				throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected KeyEntities token");
			nodes.Add(tok);
			if ((tok = nextToken()).Type != NodeType.Tok_Colon)
				throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected Colon token");
			nodes.Add(tok);
			while (true) {
				if ((tok = fetchToken(laOffset)).Type == NodeType.Tok_KeyActor || tok.Type == NodeType.Tok_KeyDestructible) {
					nodes.Add(matchEntityDef());
				}
				else
					break;
			}


			return new RuleInstance(NodeType.Rule_EntitiesSection, nodes.ToArray());
		}

		private RuleInstance matchFlagEntry() {
			List<Node> nodes = new List<Node>();
			Token tok;

			nodes.Add(matchAnyName());
			if ((tok = nextToken()).Type != NodeType.Tok_Assign)
				throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected Assign token");
			nodes.Add(tok);
			if (fetchToken(laOffset).Type == NodeType.Tok_KeyTrue) {
				if ((tok = nextToken()).Type != NodeType.Tok_KeyTrue)
					throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected KeyTrue token");
				nodes.Add(tok);
			}
			else {
				if ((tok = nextToken()).Type != NodeType.Tok_KeyFalse)
					throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected KeyFalse token");
				nodes.Add(tok);
			}


			return new RuleInstance(NodeType.Rule_FlagEntry, nodes.ToArray());
		}

		private RuleInstance matchNumberEntry() {
			List<Node> nodes = new List<Node>();
			Token tok;

			nodes.Add(matchAnyName());
			if ((tok = nextToken()).Type != NodeType.Tok_Assign)
				throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected Assign token");
			nodes.Add(tok);
			if ((tok = nextToken()).Type != NodeType.Tok_FloatLiteral)
				throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected FloatLiteral token");
			nodes.Add(tok);


			return new RuleInstance(NodeType.Rule_NumberEntry, nodes.ToArray());
		}

		private RuleInstance matchEntityDef() {
			List<Node> nodes = new List<Node>();
			Token tok;

			if (fetchToken(laOffset).Type == NodeType.Tok_KeyActor) {
				if ((tok = nextToken()).Type != NodeType.Tok_KeyActor)
					throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected KeyActor token");
				nodes.Add(tok);
			}
			else {
				if ((tok = nextToken()).Type != NodeType.Tok_KeyDestructible)
					throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected KeyDestructible token");
				nodes.Add(tok);
			}
			if ((tok = nextToken()).Type != NodeType.Tok_LPar)
				throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected LPar token");
			nodes.Add(tok);
			nodes.Add(matchAnyName());
			if ((tok = nextToken()).Type != NodeType.Tok_RPar)
				throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected RPar token");
			nodes.Add(tok);
			if ((tok = nextToken()).Type != NodeType.Tok_LBrace)
				throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected LBrace token");
			nodes.Add(tok);
			while (true) {
				laOffsets.Push(laOffset);
				laSuccess.Push(true);
				lookaheadAnyName();
				if (laSuccess.Peek()) {
					if (fetchToken(laOffset).Type == NodeType.Tok_Assign)
						laOffset++;
					else {
						laSuccess.Pop();
						laSuccess.Push(false);
					}
				}
				laOffset = laOffsets.Pop();
				if (laSuccess.Pop()) {
					nodes.Add(matchEntityProperty());
				}
				else
					break;
			}
			if (fetchToken(laOffset).Type == NodeType.Tok_KeyOverrides) {
				nodes.Add(matchOverridesBlock());
			}
			if ((tok = nextToken()).Type != NodeType.Tok_RBrace)
				throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected RBrace token");
			nodes.Add(tok);


			return new RuleInstance(NodeType.Rule_EntityDef, nodes.ToArray());
		}

		private RuleInstance matchEntityProperty() {
			List<Node> nodes = new List<Node>();

			laOffsets.Push(laOffset);
			laSuccess.Push(true);
			lookaheadAnyName();
			if (laSuccess.Peek()) {
				if (fetchToken(laOffset).Type == NodeType.Tok_Assign)
					laOffset++;
				else {
					laSuccess.Pop();
					laSuccess.Push(false);
				}
			}
			if (laSuccess.Peek()) {
				if (fetchToken(laOffset).Type == NodeType.Tok_StringLiteral)
					laOffset++;
				else {
					laSuccess.Pop();
					laSuccess.Push(false);
				}
			}
			laOffset = laOffsets.Pop();
			if (laSuccess.Pop()) {
				nodes.Add(matchEPString());
			}
			else {
				laOffsets.Push(laOffset);
				laSuccess.Push(true);
				lookaheadAnyName();
				if (laSuccess.Peek()) {
					if (fetchToken(laOffset).Type == NodeType.Tok_Assign)
						laOffset++;
					else {
						laSuccess.Pop();
						laSuccess.Push(false);
					}
				}
				if (laSuccess.Peek()) {
					if (fetchToken(laOffset).Type == NodeType.Tok_FloatLiteral)
						laOffset++;
					else {
						laSuccess.Pop();
						laSuccess.Push(false);
					}
				}
				if (laSuccess.Peek()) {
					if (fetchToken(laOffset).Type == NodeType.Tok_Comma)
						laOffset++;
					else {
						laSuccess.Pop();
						laSuccess.Push(false);
					}
				}
				if (laSuccess.Peek()) {
					if (fetchToken(laOffset).Type == NodeType.Tok_FloatLiteral)
						laOffset++;
					else {
						laSuccess.Pop();
						laSuccess.Push(false);
					}
				}
				laOffset = laOffsets.Pop();
				if (laSuccess.Pop()) {
					nodes.Add(matchEPVector3D());
				}
				else {
					laOffsets.Push(laOffset);
					laSuccess.Push(true);
					lookaheadAnyName();
					if (laSuccess.Peek()) {
						if (fetchToken(laOffset).Type == NodeType.Tok_Assign)
							laOffset++;
						else {
							laSuccess.Pop();
							laSuccess.Push(false);
						}
					}
					if (laSuccess.Peek()) {
						if (fetchToken(laOffset).Type == NodeType.Tok_FloatLiteral)
							laOffset++;
						else {
							laSuccess.Pop();
							laSuccess.Push(false);
						}
					}
					laOffset = laOffsets.Pop();
					if (laSuccess.Pop()) {
						nodes.Add(matchEPFloat());
					}
					else {
						nodes.Add(matchEPBool());
					}
				}
			}


			return new RuleInstance(NodeType.Rule_EntityProperty, nodes.ToArray());
		}

		private RuleInstance matchEPString() {
			List<Node> nodes = new List<Node>();
			Token tok;

			nodes.Add(matchAnyName());
			if ((tok = nextToken()).Type != NodeType.Tok_Assign)
				throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected Assign token");
			nodes.Add(tok);
			if ((tok = nextToken()).Type != NodeType.Tok_StringLiteral)
				throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected StringLiteral token");
			nodes.Add(tok);


			return new RuleInstance(NodeType.Rule_EPString, nodes.ToArray());
		}

		private RuleInstance matchEPVector3D() {
			List<Node> nodes = new List<Node>();
			Token tok;

			nodes.Add(matchAnyName());
			if ((tok = nextToken()).Type != NodeType.Tok_Assign)
				throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected Assign token");
			nodes.Add(tok);
			if ((tok = nextToken()).Type != NodeType.Tok_FloatLiteral)
				throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected FloatLiteral token");
			nodes.Add(tok);
			if ((tok = nextToken()).Type != NodeType.Tok_Comma)
				throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected Comma token");
			nodes.Add(tok);
			if ((tok = nextToken()).Type != NodeType.Tok_FloatLiteral)
				throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected FloatLiteral token");
			nodes.Add(tok);
			if ((tok = nextToken()).Type != NodeType.Tok_Comma)
				throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected Comma token");
			nodes.Add(tok);
			if ((tok = nextToken()).Type != NodeType.Tok_FloatLiteral)
				throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected FloatLiteral token");
			nodes.Add(tok);


			return new RuleInstance(NodeType.Rule_EPVector3D, nodes.ToArray());
		}

		private RuleInstance matchEPFloat() {
			List<Node> nodes = new List<Node>();
			Token tok;

			nodes.Add(matchAnyName());
			if ((tok = nextToken()).Type != NodeType.Tok_Assign)
				throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected Assign token");
			nodes.Add(tok);
			if ((tok = nextToken()).Type != NodeType.Tok_FloatLiteral)
				throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected FloatLiteral token");
			nodes.Add(tok);


			return new RuleInstance(NodeType.Rule_EPFloat, nodes.ToArray());
		}

		private RuleInstance matchEPBool() {
			List<Node> nodes = new List<Node>();
			Token tok;

			nodes.Add(matchAnyName());
			if ((tok = nextToken()).Type != NodeType.Tok_Assign)
				throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected Assign token");
			nodes.Add(tok);
			if (fetchToken(laOffset).Type == NodeType.Tok_KeyFalse) {
				if ((tok = nextToken()).Type != NodeType.Tok_KeyFalse)
					throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected KeyFalse token");
				nodes.Add(tok);
			}
			else {
				if ((tok = nextToken()).Type != NodeType.Tok_KeyTrue)
					throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected KeyTrue token");
				nodes.Add(tok);
			}


			return new RuleInstance(NodeType.Rule_EPBool, nodes.ToArray());
		}

		private RuleInstance matchOverridesBlock() {
			List<Node> nodes = new List<Node>();
			Token tok;

			if ((tok = nextToken()).Type != NodeType.Tok_KeyOverrides)
				throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected KeyOverrides token");
			nodes.Add(tok);
			if ((tok = nextToken()).Type != NodeType.Tok_LBrace)
				throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected LBrace token");
			nodes.Add(tok);
			while (true) {
				laOffsets.Push(laOffset);
				laSuccess.Push(true);
				lookaheadAnyName();
				if (laSuccess.Peek()) {
					if (fetchToken(laOffset).Type == NodeType.Tok_Assign)
						laOffset++;
					else {
						laSuccess.Pop();
						laSuccess.Push(false);
					}
				}
				laOffset = laOffsets.Pop();
				if (laSuccess.Pop()) {
					nodes.Add(matchEntityProperty());
				}
				else
					break;
			}
			if ((tok = nextToken()).Type != NodeType.Tok_RBrace)
				throw new ParserException("Line " + tok.LineNr + ", Char " + tok.CharNr + ": Expected RBrace token");
			nodes.Add(tok);


			return new RuleInstance(NodeType.Rule_OverridesBlock, nodes.ToArray());
		}

		private void lookaheadAnyName() {
			if (fetchToken(laOffset).Type == NodeType.Tok_Name) {
				if (fetchToken(laOffset).Type == NodeType.Tok_Name)
					laOffset++;
				else {
					laSuccess.Pop();
					laSuccess.Push(false);
				}
			}
			else {
				if (fetchToken(laOffset).Type == NodeType.Tok_KeyActor) {
					if (fetchToken(laOffset).Type == NodeType.Tok_KeyActor)
						laOffset++;
					else {
						laSuccess.Pop();
						laSuccess.Push(false);
					}
				}
				else {
					if (fetchToken(laOffset).Type == NodeType.Tok_KeyDestructible) {
						if (fetchToken(laOffset).Type == NodeType.Tok_KeyDestructible)
							laOffset++;
						else {
							laSuccess.Pop();
							laSuccess.Push(false);
						}
					}
					else {
						if (fetchToken(laOffset).Type == NodeType.Tok_KeyEntities) {
							if (fetchToken(laOffset).Type == NodeType.Tok_KeyEntities)
								laOffset++;
							else {
								laSuccess.Pop();
								laSuccess.Push(false);
							}
						}
						else {
							if (fetchToken(laOffset).Type == NodeType.Tok_KeyFalse) {
								if (fetchToken(laOffset).Type == NodeType.Tok_KeyFalse)
									laOffset++;
								else {
									laSuccess.Pop();
									laSuccess.Push(false);
								}
							}
							else {
								if (fetchToken(laOffset).Type == NodeType.Tok_KeyFlags) {
									if (fetchToken(laOffset).Type == NodeType.Tok_KeyFlags)
										laOffset++;
									else {
										laSuccess.Pop();
										laSuccess.Push(false);
									}
								}
								else {
									if (fetchToken(laOffset).Type == NodeType.Tok_KeyNumbers) {
										if (fetchToken(laOffset).Type == NodeType.Tok_KeyNumbers)
											laOffset++;
										else {
											laSuccess.Pop();
											laSuccess.Push(false);
										}
									}
									else {
										if (fetchToken(laOffset).Type == NodeType.Tok_KeyOverrides) {
											if (fetchToken(laOffset).Type == NodeType.Tok_KeyOverrides)
												laOffset++;
											else {
												laSuccess.Pop();
												laSuccess.Push(false);
											}
										}
										else {
											if (fetchToken(laOffset).Type == NodeType.Tok_KeyTrue)
												laOffset++;
											else {
												laSuccess.Pop();
												laSuccess.Push(false);
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}
	}
}
