# Presentation
This is the GitHub repo for <a href="https://www.catallaxyservices.com/presentations/dotnetml/">my presentation entitled Machine Learning with .NET</a>.

# Lab
This is also a repo for a lab.  What follows are the lab steps.

## Phase One:  Prep Work

1. Download and install the <a href="https://marketplace.visualstudio.com/items?itemName=MLNET.07">Microsoft ML.NET Model Builder</a> and install it.  This installer works for Visual Studio 2017 and Visual Studio 2019, so you will need at least one of these two installed beforehand.
2. Clone this GitHub repository or download the solution as a zip file.
3. Open the solution and ensure that you are able to build everything successfully.  You may need to change the project to use .NET Core 2.1 instead of 2.2.  If you do not already have .NET Core, grab it from [the Microsoft website](https://dotnet.microsoft.com/download).

## Phase Two:  Predict Buffalo Bills Outcomes

1. Review the three projects:  `DotNetMachineLearning`, `DotNetMachineLearning.BillsTrainer`, and `DotNetMachineLearning.Tests`.
2. Open `BillsModelTrainerTest.cs` and navigate to the `SaveAndLoadModel()` test.  It points to a directory, `C:\Temp`.  If you already have a folder named `Temp` on your C drive and you are okay with using it, you can leave this alone.  Otherwise, change the string to point to a location where you would like to save the model file.
3. In the Visual Studio Test Explorer, run all tests to ensure that your solution is configured correctly.
4. Run each of the tests and ensure that it runs correctly.
5. Review the `BasicEvaluationTest()` test function.  Add at least one more multi-class classifier not already covered.

## Phase Three:  Sentiment Analysis

Unlike phase two, we will not start with a project.

1. Right-click on the `DotNetMachineLearning.BillsModelTrainer` and go to `Add -> Machine Learning`.
2. Select "Sentiment Analysis."
3. Select Tweets.csv from the Sentiment Analysis folder.  Scroll down and select the "Train" link.
4. Keep the training time at 10 seconds and click "Start training."
5. After it completes, click the "Evaluate" link and review the results.
6. Click on the `3. Train` link and change the time from 10 seconds to a higher value.  Try a few different values.
7. When you have a viable model, click the "Code" link.  Then, click the "Add Projects" button.

Then, review `DotNetMachineLearningML.ConsoleApp`.  Set this project as your startup project and run it.

### Changing the Console App

Once you have tried out the console app, change the main method to the following:

```c#
static void Main(string[] args)
{
	MLContext mlContext = new MLContext();

	ITransformer mlModel = mlContext.Model.Load(GetAbsolutePath(MODEL_FILEPATH), out DataViewSchema inputSchema);
	var predEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);

	Console.WriteLine("Enter a sample tweet:");
	var text = Console.ReadLine();

	// Create sample data to do a single prediction with it 
	ModelInput sampleData = new ModelInput { Text = text) };

	// Try a single prediction
	ModelOutput predictionResult = predEngine.Predict(sampleData);
	string outcome = predictionResult.Prediction ? "Positive tweet" : "Negative tweet";
	Console.WriteLine($"Single Prediction --> Predicted value: {outcome}");

	Console.WriteLine("=============== End of process, hit any key to finish ===============");
	Console.ReadKey();
}
```

This will allow you to try out entering sample tweets.

# Datasets
The Buffalo Bills data set was hand-entered from the <a href="https://www.pro-football-reference.com/teams/buf/2018.htm">Pro Football Reference website</a>.  This data set is shared under the terms of the GPL 3.0 license.

The airline sentiment analysis data set is <a href="https://www.kaggle.com/crowdflower/twitter-airline-sentiment">originally from a Kaggle data set</a>.  I modified the data set to remove Neutral entries, to recode "positive" and "negative" as 1 and 0, respsectively, and to remove newline characters.  This data set is shared under terms of <a href="https://creativecommons.org/licenses/by-nc-sa/4.0/">Creative Commons CC BY-NC-SA 4.0</a>.
