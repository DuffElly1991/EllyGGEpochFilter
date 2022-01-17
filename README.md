Filter builder for Last Epoch

When launched it will monitor for changes to filter templates made in game, and build filters from these templates

----------------------------------------------------------------------------------
To make your own filter in game:
  Select the filter "EllyGG Settings"

  Create a new rule (Add Rule button) or edit an existing template (click on it in the list)

  Set the class
  - Click add condition
  - Select Class Requirement in condition dropdown
  - Select the class in dropdown

  Set the items you want
  - Click add condition
  - Select item type in condition dropdown
  - Select the item types you want in type dialogue

    (Subtypes are not set due to a limitation in the editor: subtypes are only selectable if you have just one item type.
      Because of this the filter builder will guess at what the best subtype is.  Other subtypes are shown as well if they have enough affixes)

  Set the affixes you want
  - Click add condition
  - Select affix in condition dropdown
  - Add affixes in the affix dialogue

      (You do not need to specify number on item or any of the advanced settings)

  Optional: change the rune of shattering targets rule to control what items to show for shattering

----------------------------------------------------------------------------------
Setup instructions from source code:

Find Last Epoch's filter folder and add the path to this folder to 'Settings.xml'

Put 'EllyGG_Settings.xml' in Last Epoch's filter folder

Compile and Run the program with your C# editor of choice

----------------------------------------------------------------------------------
Setup instructions from zip folder (windows):

Extract the zip folder to whereever you want

Find Last Epoch's filter folder and add the path to this folder to 'Settings.xml'

Put 'EllyGG_Settings.xml' in Last Epoch's filter folder

Run EllyGGEpochFilter.exe

----------------------------------------------------------------------------------
Setup instructions from zip folder (linux):

Extract the zip folder to whereever you want

Find Last Epoch's filter folder and add the path to this folder to 'Settings.xml'

Put 'EllyGG_Settings.xml' in Last Epoch's filter folder

Flag 'EllyGGEpochFilter' as executable, then run it
