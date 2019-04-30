using NUnit.Framework;
using DotNetMachineLearning.BillsNaiveBayes;
using Microsoft.ML;
using Microsoft.ML.Data;
using System;

namespace Tests
{
	public class Tests
	{
		MLContext mlContext;
		BillsModelTrainer bmt;
		ITransformer model;
		PredictionEngineBase<RawInput, Prediction> predictor;
		

		[SetUp]
		public void Setup()
		{
			mlContext = new MLContext(seed: 9997);
			bmt = new BillsModelTrainer();

			var data = bmt.GetRawData(mlContext, "Resources\\2018Bills.csv");
			model = bmt.TrainModel(mlContext, data);

			predictor = model.CreatePredictionEngine<RawInput, Prediction>(mlContext);
		}

		[TestCase(new object[] { "Josh Allen", "Home", 17, "Robert Foster", "LeSean McCoy", "Win" })]
		[TestCase(new object[] { "Josh Allen", "Away", 17, "Robert Foster", "LeSean McCoy", "Win" })]
		[TestCase(new object[] { "Josh Allen", "Home", 17, "Kelvin Benjamin", "LeSean McCoy", "Loss" })]
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

			var newPredictor = newModel.CreatePredictionEngine<RawInput, Prediction>(mlContext);
			var po = GenerateOutcome(predictor);
			var npo = GenerateOutcome(newPredictor);

			Assert.IsNotNull(newModel);
			Assert.AreEqual(po, npo);
		}
	}
}