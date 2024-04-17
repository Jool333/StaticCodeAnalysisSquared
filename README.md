A tool to help automate data collection from SAST scans.
Built with a basic console ui.
Contains different sub features such as making a workflow file for a selection of juliet testcases, condensing and sorting CWE rules for a tool, find and collect the good and bad methods in the juliet test suite, connecting to the api of sonarqube or github in order to extract the data, then analyzing the data compared to the found good and bad methods to determine true and false positives and negatives.

The workflow maker works by recursively jumping through the testcase directory to find the .csproj files and uses it to fill out a template for each workflow line. All lines are then printed in the console.

The rule condenser is a relatively basic tool, it looks through a txt file with a tools listed CWE rules, then filters out any duplicates and sorts the list and prints it in the console.

The part that identifies good and bad methods in testcases works by jumping recursively though the testcase directory. Once it finds any files in a directory it then goes though any .cs file as long as its not a base/neutral class or a program class. Inside the file it goes line by line to find the start and end line of any good or bad methods. This is possible as each good and bad method is written in a consistent way and the inclusion of regions in the code that tell the start an end of a good/bad segment.  When the whole file is processed this way, it creates good and bad objects respectively and adds them to their own lists. Along with the files name, the testcase the lists make up a GoodBadEntity object, these are created for every file and added to a list to be used for analysis. 

Data collection
The two scanners work in a similar way to each other, with slight differences. They start by getting any needed secret data such as tokens, users and owners. Then using these secrets fetches the desired security scanning data. As the data occupies several pages it systematically gets each available page, noting the progress of fetching for each page. The data is deserialized, made into an object and added to a list of all found hotspots. The hotspot list is then filtered to remove any issues that occupy the same line in the same file. There then is a second filter that removes any unwanted hotspot types. Once all filtration is done the list is ordered by testcase and line number. As the data collected from SonarQube vs GitHub is slightly different they have different hotspot classes, but contain approximately the same information, the file path/component, line number and any related rules/messages. Incase any file that wasn’t intended was scanned there is a final filter to only use names that include CWE. The total found hotspots is printed. Next it prints the unique hotspots found within each testcase.
Now the analysis can begin. 
Each testcase file is looped through and sees if there is a matching hotspot for that file, and what that hotspots lines are. A clone of the current files good and bad list is created without reference as its needed to have only one found issue per good/bad method, more on that later.
If there was a matched hotspot and it had any lines in it the lines are looped through. 
In each loop:
1. It starts by checking if there are any bad methods for the file.
   - If yes: loops the cloned bad list checks if the current line is within the span of the bad methods lines.
     - If yes it increments the true positive count and removes the bad object from the cloned list, and the loop starts over. 
      - If no continues the loop to the next bad object.
   - If no continues to the good check below.

2.	Checks if there are any good methods for the file.
    - If yes: loops the cloned good list checks if the current line is within the span of the good methods lines.
      - If yes: increments false positives and removes the good object from the cloned list, loop continues.
      - If no: continues the loop to the next good object.
    - If no: increments the duplicate amount.

That is the end of the line loop. But the file loop continues, it now adds the number of remaining objects in the cloned good list to the true negative count and the number of remaining objects in the cloned bad list to the false negatives.
If there had been no line /matched hotspots to the current file similarly the good and bad list clones count would have been added to the true negatives and false negatives respectively. The loop then continues to the next file until all have been processed.

Once done we have the final tally of tp, fp, tn and fn which will be used to calculate our result data, precision, recall, F1-score and MCC. This calculation is done in the code and then printed.
