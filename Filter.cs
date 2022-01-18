using System;
using System.Collections.Generic;

namespace EllyGGEpochFilter
{
	public class Filter
	{
		static string filterVersion = "2.0";
		static string lastModifiedInVersion = null;
		static List<xmlNode> levelingRules;
		static List<Equipment> bestEquipment;
		static Equipment bestIdols;
		static xmlNode desirableItemAffixes = null;
		static xmlNode runeOfShatteringTargets = null;

		public static bool LoadGatheredInformationFromXML(XmlManager settingsXML, out string outErrorMessage)
		{
			levelingRules = new List<xmlNode>();
			bestEquipment = new List<Equipment>();

			//Validate that we have the right file
			xmlNode itemFilter = settingsXML.getXml();
			if(itemFilter.label != "ItemFilter")
			{
				outErrorMessage = "Expected to find root node <ItemFilter>, but found <" + itemFilter.label + "> (Is this the wrong file?)";
				return false;
			}

			xmlNode rules = null;
			foreach(xmlNode child in itemFilter.children)
			{
				if(child.label == "name")
				{
					if(child.value != "EllyGG_Information_Gatherer")
					{
						outErrorMessage = "Expected to find item filter called \"EllyGG_Information_Gatherer\", but found \"" + child.value + "\"";
						return false;
					}
				}

				if(child.label == "rules")
				{
					rules = child;
				}
			}

			//Loop over rules
			foreach(xmlNode rule in rules.children)
			{
				//Check child is a rule
				if(rule.label != "Rule")
				{
					outErrorMessage = "Expected to find node <Rule> under node <rules>, but instead found <" + rule.label + ">";
					return false;
				}

				//Get the name of the rule
				string ruleName = null;
				if(rule.children != null)
				{
					foreach(xmlNode child in rule.children)
					{
						if(child.label == "nameOverride")
						{
							ruleName = child.value;
						}
					}
				}

				//Find out the rule type
				if(ruleName == null)
				{
					//Ignore this rule, assume that it is something that the user is currently editing
				}
				else if(ruleName.StartsWith("Leveling items - "))
				{
					//This is a global rule containing items used for leveling
					levelingRules.Add(rule);
				}
				else if(ruleName.StartsWith("Best"))
				{
					//This is a global setting defining the best item subtype for a given item type
					string type = null;
					xmlNode condition = null;

					foreach(xmlNode i in rule.children)
					{
						if(i.label == "conditions")
						{
							foreach(xmlNode j in i.children)
							{
								if(j.label == "Condition" && j.value.Contains("SubTypeCondition"))
								{
									condition = j;
									foreach(xmlNode k in condition.children)
									{
										if(k.label == "type"
											&& k.children != null
											&& k.children.Count == 1
											&& k.children[0].label == "EquipmentType")
										{
											type = k.children[0].value;
											break;
										}
										else if(k.label == "type"
											&& k.children != null
											&& k.children.Count > 1
											&& k.children[0].label == "EquipmentType"
											&& k.children[0].value.StartsWith("IDOL"))
											{
												type = "IDOL";
											}
									}
									break;
								}
							}
							break;
						}
					}

					if(type == null || condition == null)
					{
						//Assume that this rule is currently being created and do nothing
					}
					else if(type == "IDOL")
					{
						bestIdols = new Equipment(type, condition);
					}
					else
					{
						Equipment currentEquipment = new Equipment(type, condition);
						bestEquipment.Add(currentEquipment);
					}
				}
				else if(ruleName == "Desirable Item Affixes")
				{
					foreach(xmlNode i in rule.children)
					{
						if(i.label == "conditions" && i.children != null)
						{

							foreach(xmlNode j in i.children)
							{
								if(j.label == "Condition" && j.value.Contains("AffixCondition") && j.children != null)
								{
									foreach(xmlNode k in j.children)
									{
										if(k.label == "affixes")
										{
											desirableItemAffixes = k;
											break;
										}
									}
									break;
								}
							}
							break;
						}
					}
				}
			}

			outErrorMessage = "";
			return true;
		}

		public static bool LoadFilterSpecificationsFromSettingsXML(XmlManager settingsXML, out string outErrorMessage, out List<(string name, string xml)> outFilters)
		{
			outFilters = new List<(string name, string xml)>();
			List<Filter> filters = new List<Filter>();

			//Validate that we have the right file
			xmlNode itemFilter = settingsXML.getXml();
			if(itemFilter.label != "ItemFilter")
			{
				outErrorMessage = "Expected to find root node <ItemFilter>, but found <" + itemFilter.label + "> (Is this the wrong file?)";
				return false;
			}

			xmlNode rules = null;
			foreach(xmlNode child in itemFilter.children)
			{
				if(child.label == "name")
				{
					if(child.value != "EllyGG_Settings")
					{
						outErrorMessage = "Expected to find item filter called \"EllyGG_Settings\", but found \"" + child.value + "\"";
						return false;
					}
				}

				if(child.label == "rules")
				{
					rules = child;
				}

				if(child.label == "lastModifiedInVersion")
				{
					lastModifiedInVersion = child.value;
				}
			}	

			//Loop over rules
			foreach(xmlNode rule in rules.children)
			{
				//Check child is a rule
				if(rule.label != "Rule")
				{
					outErrorMessage = "Expected to find node <Rule> under node <rules>, but instead found <" + rule.label + ">";
					return false;
				}

				//Get the name of the rule
				string ruleName = null;
				if(rule.children != null)
				{
					foreach(xmlNode child in rule.children)
					{
						if(child.label == "nameOverride")
						{
							ruleName = child.value;
						}
					}
				}

				//Find out the rule type
				if(ruleName == null)
				{
					//Ignore this rule, assume that it is something that the user is currently editing
				}
				else if(ruleName == "Rune of shattering targets")
				{
					runeOfShatteringTargets = rule;
				}
				else if(ruleName.StartsWith("example filter") || ruleName.StartsWith("Example filter"))
				{
					//This is an example rule for use as a template, don't create a filter for it
				}
				else
				{
					//This is a filter rule
					bool isEnabled = false;
					string characterClass = null;
					xmlNode equipment = null;
					xmlNode affixes = null;

					foreach(xmlNode i in rule.children)
					{
						if(i.label == "conditions" && i.children != null)
						{
							foreach(xmlNode j in i.children)
							{
								if(j.label == "Condition")
								{
									if(j.value.Contains("ClassCondition") && j.children != null)
									{
										characterClass = j.children[0].value;
									}
									else if(j.value.Contains("SubTypeCondition") && j.children != null)
									{
										equipment = j;
									}
									else if(j.value.Contains("AffixCondition") && j.children != null)
									{
										foreach(xmlNode k in j.children)
										{
											if(k.label == "affixes")
											{
												affixes = k;
											}
										}
									}
								}
							}
						}
						if(i.label == "isEnabled" && i.value == "true")
						{
							isEnabled = true;
						}
					}

					if(isEnabled && characterClass != null && equipment != null && affixes != null)
					{
						Filter current = new Filter(ruleName, characterClass, equipment, affixes);
						filters.Add(current);
					}
				}
			}

			//Now that we have our list of filters, create files for them
			foreach(Filter currentFilter in filters)
			{
				xmlNode filterContent = currentFilter.buildFilter();

				outFilters.Add((currentFilter.formattedName + ".xml", filterContent.ToString()));
			}

			outErrorMessage = "";
			return true;
		}

		static xmlNode createHideFilterRule()
		{
			return createFilterRule(RuleType.HIDE, null, null, true, false, 0, 0, false, null);
		}

		static xmlNode createHideFilterRule(xmlNode conditions, string name)
		{
			return createFilterRule(RuleType.HIDE, conditions, null, true, false, 0, 0, false, name);
		}

		static xmlNode createShowFilterRule(xmlNode conditions, string name)
		{
			return createFilterRule(RuleType.SHOW, conditions, null, true, false, 0, 0, false, name);
		}
		static xmlNode createShowFilterRule(xmlNode conditions, int minLevel, int maxLevel, string name)
		{
			return createFilterRule(RuleType.SHOW, conditions, null, true, (maxLevel > 0), minLevel, maxLevel, false, name);
		}

		static xmlNode createHighlightFilterRule(string colour, xmlNode conditions, string name)
		{
			return createFilterRule(RuleType.HIGHLIGHT, conditions, colour, true, false, 0, 0, false, name);
		}

		static xmlNode createFilterRule(RuleType type, xmlNode conditions, string colour, bool isEnabled, bool isLevelDependent, int minLevel, int maxLevel, bool isEmphasized, string name)
		{
			XmlManager xmlManager = new XmlManager();

			xmlManager.startDualTagMultiLine("Rule", null);
			{
				xmlManager.addDualTagSingleLine("type", type.ToString());
				if(conditions == null)
				{
					xmlManager.addSingleTag("conditions", null);
				}
				else
				{
					xmlManager.addNode(conditions.Clone());
				}

				if(colour == null)
				{
					colour = "0";
				}
				xmlManager.addDualTagSingleLine("color", colour);

				//This is done as toString() makes booleans uppercase
				if(isEnabled)
				{
					xmlManager.addDualTagSingleLine("isEnabled", "true");
				}
				else
				{
					xmlManager.addDualTagSingleLine("isEnabled", "false");
				}

				if(isLevelDependent)
				{
					xmlManager.addDualTagSingleLine("levelDependent", "true");
				}
				else
				{
					xmlManager.addDualTagSingleLine("levelDependent", "false");
				}

				xmlManager.addDualTagSingleLine("minLvl", minLevel.ToString());

				xmlManager.addDualTagSingleLine("maxLvl", maxLevel.ToString());

				if(isEmphasized)
				{
					xmlManager.addDualTagSingleLine("emphasized", "true");
				}
				else
				{
					xmlManager.addDualTagSingleLine("emphasized", "false");
				}

				if(name == null || name == "")
				{
					xmlManager.addSingleTag("nameOverride", null);
				}
				else
				{
					xmlManager.addDualTagSingleLine("nameOverride", name);
				}
			}
			xmlManager.endDualTagMultiLine();

			return xmlManager.getXml();
		}

		static xmlNode combineConditions(xmlNode baseTypeCondition, xmlNode affixCondition)
		{
			XmlManager conditions = new XmlManager();
			conditions.startDualTagMultiLine("conditions", null);
			{
				if(baseTypeCondition != null)
				{
					conditions.addNode(baseTypeCondition.Clone());
				}
				if(affixCondition != null)
				{
					conditions.addNode(affixCondition.Clone());
				}
			}
			conditions.endDualTagMultiLine();

			return conditions.getXml();
		}

		static xmlNode createAffixCondition(xmlNode affixes, ComparisonType comparison, int comparisonLevel, int minUniqueAffixesRequired)
		{
			XmlManager xmlManager = new XmlManager();
			xmlManager.startDualTagMultiLine("Condition", "i:type=\"AffixCondition\"");
			{
				xmlManager.addNode(affixes.Clone());
				xmlManager.addDualTagSingleLine("comparsion", comparison.ToString());
				xmlManager.addDualTagSingleLine("comparsionValue", comparisonLevel.ToString());
				xmlManager.addDualTagSingleLine("minOnTheSameItem", minUniqueAffixesRequired.ToString());
				xmlManager.addDualTagSingleLine("advanced", "true");
			}
			xmlManager.endDualTagMultiLine();

			return xmlManager.getXml();
		}

		static xmlNode createAffixCondition(xmlNode affixes, CombinedComparisonType combinedComparison, int combinedComparisonLevel)
		{
			XmlManager xmlManager = new XmlManager();
			xmlManager.startDualTagMultiLine("Condition", "i:type=\"AffixCondition\"");
			{
				xmlManager.addNode(affixes.Clone());
				xmlManager.addDualTagSingleLine("comparsion", "ANY");
				xmlManager.addDualTagSingleLine("comparsionValue", "0");
				xmlManager.addDualTagSingleLine("minOnTheSameItem", "1");
				xmlManager.addDualTagSingleLine("combinedComparsion", combinedComparison.ToString());
				xmlManager.addDualTagSingleLine("combinedComparsionValue", combinedComparisonLevel.ToString());
				xmlManager.addDualTagSingleLine("advanced", "true");
			}
			xmlManager.endDualTagMultiLine();

			return xmlManager.getXml();
		}

		public string rawName;
		public string formattedName;
		public string characterClass;
		public xmlNode equipment;
		public xmlNode affixes;

		private xmlNode idolSubTypeCondition;
		private List<Equipment> equipmentSubTypeConditions;

		public Filter(string inputName, string inputClass, xmlNode inputEquipment, xmlNode inputAffixes)
		{
			rawName = inputName;
			characterClass = inputClass;
			equipment = inputEquipment;
			affixes = inputAffixes;

			formattedName = inputName;
			if(formattedName.StartsWith("Filter for "))
			{
				formattedName = "f" + formattedName.Substring(1);
			}
			if(!formattedName.StartsWith("filter for "))
			{
				formattedName = "filter for " + formattedName;
			}
			formattedName = "EllyGG " + formattedName;
		}

		public xmlNode buildFilter()
		{
			//Build base types by adding in the best subtypes
			createIdolSubTypeCondition();
			createEquipmentSubTypeConditions();

			xmlNode affixCondition = null;
			xmlNode conditions = null;
			string name = null;
			
			XmlManager filterXML = new XmlManager();
			filterXML.startDualTagMultiLine("ItemFilter", "xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\"");
			{
				filterXML.addDualTagSingleLine("name", formattedName);
				filterXML.addDualTagSingleLine("filterIcon", "23");
				filterXML.addDualTagSingleLine("filterIconColor", "13");
				filterXML.addDualTagSingleLine("description", "Elly's Good Gaming Epoch Filter, generated from specification '" + rawName + "' in loot filter 'EllyGG Settings'.  Filter version " + Filter.filterVersion);
				filterXML.addDualTagSingleLine("lastModifiedInVersion", Filter.lastModifiedInVersion);
				
				filterXML.startDualTagMultiLine("rules", null);
				{
					//(rules at the top of file apply last)

					//Hide all items rule
					filterXML.addNode(Filter.createHideFilterRule());

					//Show idols with desired affixes
					affixCondition = Filter.createAffixCondition(affixes, ComparisonType.ANY, 0, 1);
					conditions = Filter.combineConditions(idolSubTypeCondition, affixCondition);
					filterXML.addNode(Filter.createShowFilterRule(conditions, "Idols with relevant affixes"));

					//Show idols with 2 desired affixes
					affixCondition = Filter.createAffixCondition(affixes, ComparisonType.ANY, 0, 2);
					conditions = Filter.combineConditions(idolSubTypeCondition, affixCondition);
					filterXML.addNode(Filter.createHighlightFilterRule("8", conditions, "Idols with 2 relevant affixes"));

					//Add all of the leveling rules
					foreach(xmlNode i in Filter.levelingRules)
					{
						filterXML.addNode(i.Clone());
					}

					//Filtering for items with combined affix tier for certain levels
					List<(int minLevel, int maxLevel, int combineComparisonLevel)> tiering = new List<(int minLevel, int maxLevel, int combineComparisonLevel)>();
					tiering.Add((0, 10, 1));
					tiering.Add((0, 20, 2));
					tiering.Add((0, 30, 3));
					tiering.Add((0, 40, 4));
					tiering.Add((0, 50, 5));
					tiering.Add((0, 60, 6));
					tiering.Add((0, 70, 7));
					tiering.Add((0, 80, 8));
					tiering.Add((0, 90, 9));

					foreach((int minLevel, int maxLevel, int combineComparisonLevel) tier in tiering)
					{
						affixCondition = Filter.createAffixCondition(affixes, CombinedComparisonType.MORE_OR_EQUAL, tier.combineComparisonLevel);
						conditions = new xmlNode(null, NodeType.dualTagMultiLine, "conditions", null);
						conditions.addNode(affixCondition);
						name = "Items with affixes of combined tier " + tier.combineComparisonLevel.ToString() + "+";
						if(tier.maxLevel != 0)
						{
							name += ", below level " + tier.maxLevel.ToString();
						}
						filterXML.addNode(Filter.createShowFilterRule(conditions, tier.minLevel, tier.maxLevel, name));
					}

					//Each best equipment type with tier 5
					foreach(Equipment equipmentSubType in equipmentSubTypeConditions)
					{
						affixCondition = Filter.createAffixCondition(affixes, ComparisonType.MORE_OR_EQUAL, 5, 1);
						conditions = Filter.combineConditions(equipmentSubType.xml, affixCondition);
						name = "Best " + equipmentSubType.type + " with a tier 5 affix";
						filterXML.addNode(Filter.createShowFilterRule(conditions, name));
					}

					//Highlight with tier 12+
					affixCondition = Filter.createAffixCondition(affixes, CombinedComparisonType.MORE_OR_EQUAL, 12);
					conditions = new xmlNode(null, NodeType.dualTagMultiLine, "conditions", null);
					conditions.addNode(affixCondition);
					name = "Highlight items with affixes of combined tier 12+";
					filterXML.addNode(Filter.createHighlightFilterRule("7", conditions, name));

					//Highlight with two tier 5s
					affixCondition = Filter.createAffixCondition(affixes, ComparisonType.MORE_OR_EQUAL, 5, 2);
					conditions = new xmlNode(null, NodeType.dualTagMultiLine, "conditions", null);
					conditions.addNode(affixCondition);
					name = "Highlight items with two tier 5+ affixes";
					filterXML.addNode(Filter.createHighlightFilterRule("7", conditions, name));

					//Hide equipment the player doesn't desire
					List<string> unwantedEquipment = new List<string>();
					foreach(Equipment i in Filter.bestEquipment)
					{
						bool isWanted = false;
						foreach(Equipment j in equipmentSubTypeConditions)
						{
							if(i.type == j.type)
							{
								isWanted = true;
								break;
							}
						}
						if(!isWanted && !unwantedEquipment.Contains(i.type))
						{
							unwantedEquipment.Add(i.type);
						}
					}
					if(unwantedEquipment.Count > 0)
					{
						XmlManager unwantedEquipmentXML = new XmlManager();
						unwantedEquipmentXML.startDualTagMultiLine("conditions", null);
						{
							unwantedEquipmentXML.startDualTagMultiLine("Condition", "i:type=\"SubTypeCondition\"");
							{
								unwantedEquipmentXML.startDualTagMultiLine("type", null);
								{
									foreach(string s in unwantedEquipment)
									{
										unwantedEquipmentXML.addDualTagSingleLine("EquipmentType", s);
									}
								}
								unwantedEquipmentXML.endDualTagMultiLine();
								unwantedEquipmentXML.addSingleTag("subTypes", null);
							}
							unwantedEquipmentXML.endDualTagMultiLine();
						}
						unwantedEquipmentXML.endDualTagMultiLine();

						filterXML.addNode(Filter.createHideFilterRule(unwantedEquipmentXML.getXml(), "unwanted equipment"));
					}

					//Hide wrong character class
					List<string> allClasses = new List<string>();
					allClasses.Add("Acolyte");
					allClasses.Add("Primalist");
					allClasses.Add("Mage");
					allClasses.Add("Sentinel");
					allClasses.Add("Rogue");

					string wrongClasses = null;
					foreach(string currentClass in allClasses)
					{
						if(currentClass != characterClass)
						{
							if(wrongClasses == null)
							{
								wrongClasses = currentClass;
							}
							else
							{
								wrongClasses += " " + currentClass;
							}
						}
					}
					if(wrongClasses != null)
					{
						XmlManager wrongClassXML = new XmlManager();
						wrongClassXML.startDualTagMultiLine("conditions", null);
						{
							wrongClassXML.startDualTagMultiLine("Condition", "i:type=\"ClassCondition\"");
							{
								wrongClassXML.addDualTagSingleLine("req", wrongClasses);
							}
							wrongClassXML.endDualTagMultiLine();
						}
						wrongClassXML.endDualTagMultiLine();

						filterXML.addNode(Filter.createHideFilterRule(wrongClassXML.getXml(), "items for other classes"));
					}

					//Rune of shattering worthy affixes
					filterXML.addNode(Filter.runeOfShatteringTargets.Clone());

					//Item with tier 16+ any affix
					conditions = new xmlNode(null, NodeType.dualTagMultiLine, "conditions", null);
					conditions.addNode(Filter.createAffixCondition(Filter.desirableItemAffixes, CombinedComparisonType.MORE_OR_EQUAL, 16));
					filterXML.addNode(Filter.createHighlightFilterRule("13", conditions, "Any item with affixes of combined tier 16+"));

					//Highlight items with tier 20+
					conditions = new xmlNode(null, NodeType.dualTagMultiLine, "conditions", null);
					conditions.addNode(Filter.createAffixCondition(Filter.desirableItemAffixes, CombinedComparisonType.MORE_OR_EQUAL, 20));
					filterXML.addNode(Filter.createHighlightFilterRule("12", conditions, "Any item with affixes of combined tier 20+"));

					//Unique, set and exalted items
					{
						XmlManager specialItemsXML = new XmlManager();
						specialItemsXML.startDualTagMultiLine("conditions", null);
						{
							specialItemsXML.startDualTagMultiLine("Condition", "i:type=\"RarityCondition\"");
							{
								specialItemsXML.addDualTagSingleLine("rarity", "UNIQUE SET EXALTED");
							}
							specialItemsXML.endDualTagMultiLine();
						}
						specialItemsXML.endDualTagMultiLine();

						filterXML.addNode(Filter.createShowFilterRule(specialItemsXML.getXml(), "Show Unique, Set and Exalted items"));
					}
				}
			}
			filterXML.endDualTagMultiLine();

			return filterXML.getXml();
		}

		private void createIdolSubTypeCondition()
		{
			List<string> filterIdols = new List<string>();

			foreach(xmlNode i in equipment.children)
			{
				if(i.label == "type" && i.children != null)
				{
					foreach(xmlNode j in i.children)
					{
						if(j.label == "EquipmentType" && j.value.Contains("IDOL"))
						{
							filterIdols.Add(j.value);
						}
					}
				}
			}

			if(filterIdols.Count == 0)
			{
				//User didn't add in any idols, so use the default
				idolSubTypeCondition = bestIdols.xml.Clone();
			}
			else
			{
				XmlManager xmlManager = new XmlManager();
				xmlManager.startDualTagMultiLine("Condition", "i:type=\"SubTypeCondition\"");
				{
					xmlManager.startDualTagMultiLine("type", null);
					{
						foreach(string value in filterIdols)
						{
							xmlManager.addDualTagSingleLine("EquipmentType", value);
						}
					}
					xmlManager.addSingleTag("subTypes", null);
				}
				xmlManager.endDualTagMultiLine();

				idolSubTypeCondition = xmlManager.getXml();
			}
		}

		private void createEquipmentSubTypeConditions()
		{
			List<string> etList = new List<string>();

			foreach(xmlNode i in equipment.children)
			{
				if(i.label == "type")
				{
					foreach(xmlNode j in i.children)
					{
						etList.Add(j.value);
					}
				}
			}

			equipmentSubTypeConditions = new List<Equipment>();
			foreach(string equipmentType in etList)
			{
				xmlNode subType = null;
				//Get the subtype from best equipment
				foreach(Equipment i in Filter.bestEquipment)
				{
					if(i.type == equipmentType)
					{
						foreach(xmlNode j in i.xml.children)
						{
							if(j.label == "subTypes")
							{
								subType = j;
								break;
							}
						}
						break;
					}
				}
				if(subType == null)
				{
					subType = new xmlNode(null, NodeType.singleTag, "subtypes", null);
				}

				//Create a new condition for each equipment type
				XmlManager xmlManager = new XmlManager();
				xmlManager.startDualTagMultiLine("Condition", "i:type=\"SubTypeCondition\"");
				{
					xmlManager.startDualTagMultiLine("type", null);
					{
						xmlManager.addDualTagSingleLine("EquipmentType", equipmentType);
					}
					xmlManager.endDualTagMultiLine();

					xmlManager.addNode(subType.Clone());
				}
				xmlManager.endDualTagMultiLine();

				equipmentSubTypeConditions.Add(new Equipment(equipmentType, xmlManager.getXml()));
			}
		}
	}
}