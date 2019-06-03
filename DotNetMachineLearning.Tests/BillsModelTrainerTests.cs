using NUnit.Framework;
using DotNetMachineLearning.BillsNaiveBayes;
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Diagnostics;
using System.Linq;

namespace Tests
{
	public class Tests
	{
		MLContext mlContext;
		BillsModelTrainer bmt;
		IEstimator<ITransformer> trainer;
		ITransformer model;
		PredictionEngineBase<RawInput, Prediction> predictor;
		

		[SetUp]
		public void Setup()
		{
			mlContext = new MLContext(seed: 9997);
			bmt = new BillsModelTrainer();

			var data = bmt.GetRawData(mlContext, "Resources\\2018Bills.csv");
			var split = mlContext.Data.TrainTestSplit(data, testFraction: 0.25);

			trainer = mlContext.MulticlassClassification.Trainers.NaiveBayes(labelColumnName: "Label", featureColumnName: "Features");
			model = bmt.TrainModel(mlContext, split.TrainSet, trainer);

			predictor = mlContext.Model.CreatePredictionEngine<RawInput, Prediction>(model);
		}

		[TestCase(new object[] { "Naive Bayes" })]
		[TestCase(new object[] { "L-BFGS" })]
		[TestCase(new object[] { "SDCA Non-Calibrated" })]
		public void BasicEvaluationTest(string trainerToUse)
		{
			mlContext = new MLContext(seed: 9997);
			bmt = new BillsModelTrainer();

			var data = bmt.GetRawData(mlContext, "Resources\\2018Bills.csv");
			var split = mlContext.Data.TrainTestSplit(data, testFraction: 0.4);

			// If we wish to review the split data, we can run these.
			var trainSet = mlContext.Data.CreateEnumerable<RawInput>(split.TrainSet, reuseRowObject: false);
			var testSet = mlContext.Data.CreateEnumerable<RawInput>(split.TestSet, reuseRowObject: false);

			IEstimator<ITransformer> newTrainer;
			switch (trainerToUse)
			{
				case "Naive Bayes":
					newTrainer = mlContext.MulticlassClassification.Trainers.NaiveBayes(labelColumnName: "Label", featureColumnName: "Features");
					break;
				case "L-BFGS":
					newTrainer = mlContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy(labelColumnName: "Label", featureColumnName: "Features");
					break;
				case "SDCA Non-Calibrated":
					newTrainer = mlContext.MulticlassClassification.Trainers.SdcaNonCalibrated(labelColumnName: "Label", featureColumnName: "Features");
					break;
				default:
					newTrainer = mlContext.MulticlassClassification.Trainers.NaiveBayes(labelColumnName: "Label", featureColumnName: "Features");
					break;
			}

			var newModel = bmt.TrainModel(mlContext, split.TrainSet, newTrainer);
			var metrics = mlContext.MulticlassClassification.Evaluate(newModel.Transform(split.TestSet));

			Console.WriteLine($"Macro Accuracy = {metrics.MacroAccuracy}; Micro Accuracy = {metrics.MicroAccuracy}");
			Console.WriteLine($"Confusion Matrix with {metrics.ConfusionMatrix.NumberOfClasses} classes.");
			Console.WriteLine($"{metrics.ConfusionMatrix.GetFormattedConfusionTable()}");

			Assert.AreNotEqual(0, metrics.MacroAccuracy);
		}

		[Test()]
		public void BasicCrossValidationTest()
		{
			mlContext = new MLContext(seed: 9997);
			bmt = new BillsModelTrainer();

			var data = bmt.GetRawData(mlContext, "Resources\\2018Bills.csv");
			var pipeline = bmt.GetPipeline(mlContext, trainer);
			var cvResults = mlContext.MulticlassClassification.CrossValidate(data, pipeline, numberOfFolds: 4);

			var microAccuracies = cvResults.Select(r => r.Metrics.MicroAccuracy);
			Console.WriteLine(microAccuracies.Average());
		}

		[TestCase(new object[] { "Josh Allen", "Home", 17, "Robert Foster", "LeSean McCoy", "Win" })]
		[TestCase(new object[] { "Josh Allen", "Away", 17, "Robert Foster", "LeSean McCoy", "Win" })]
		[TestCase(new object[] { "Josh Allen", "Home", 17, "Kelvin Benjamin", "LeSean McCoy", "Win" })]
		[TestCase(new object[] { "Nathan Peterman", "Home", 17, "Kelvin Benjamin", "LeSean McCoy", "Loss" })]
		[TestCase(new object[] { "Josh Allen", "Away", 7, "Charles Clay", "LeSean McCoy", "Loss" })]
		public void TestModel(string quarterback, string location, float numberOfPointsScored,
			string topReceiver, string topRunner, string expectedOutcome)
		{
			var outcome = predictor.Predict(new RawInput
			{
				Game = 0,
				Quarterback = quarterback,
				Location = location,
				NumberOfPointsScored = numberOfPointsScored,
				TopReceiver = topReceiver,
				TopRunner = topRunner,
				NumberOfSacks = 0,
				NumberOfDefensiveTurnovers = 0,
				MinutesPossession = 0,
				Outcome = "WHO KNOWS?"
			});

			Assert.AreEqual(expectedOutcome, outcome.Outcome);
		}

		private string GenerateOutcome(PredictionEngineBase<RawInput, Prediction> pe)
		{
			return pe.Predict(new RawInput
			{
				Game = 0,
				Quarterback = "Josh Allen",
				Location = "Home",
				NumberOfPointsScored = 17,
				TopReceiver = "Robert Foster",
				TopRunner = "Josh Allen",
				NumberOfSacks = 0,
				NumberOfDefensiveTurnovers = 0,
				MinutesPossession = 0,
				Outcome = "WHO KNOWS?"
			}).Outcome;
		}

		[Test()]
		public void SaveAndLoadModel()
		{
			string modelPath = "C:\\Temp\\BillsModel.mdl";
			bmt.SaveModel(mlContext, model, modelPath);

			// Register the assembly that contains 'QBCustomMappings' with the ComponentCatalog
			// so it can be found when loading the model.
			mlContext.ComponentCatalog.RegisterAssembly(typeof(QBCustomMappings).Assembly);
			mlContext.ComponentCatalog.RegisterAssembly(typeof(PointsCustomMappings).Assembly);

			var newModel = bmt.LoadModel(mlContext, modelPath);

			var newPredictor = mlContext.Model.CreatePredictionEngine<RawInput, Prediction>(newModel);
			var po = GenerateOutcome(predictor);
			var npo = GenerateOutcome(newPredictor);

			Assert.IsNotNull(newModel);
			Assert.AreEqual(po, npo);
		}
	}
}