using NUnit.Framework;
using DotNetMachineLearning.BillsNaiveBayes;
using Microsoft.ML;
using Microsoft.ML.Data;
using System;

namespace Tests
{
	public class Tests
	{
		PredictionEngineBase<RawInput, Prediction> predictor;

		[SetUp]
		public void Setup()
		{
			MLContext mlContext = new MLContext(seed: 9997);
			BillsModelTrainer bmt = new BillsModelTrainer();

			var data = bmt.GetRawData(mlContext, "C:\\Temp\\2018Bills.csv");
			var model = bmt.TrainModel(mlContext, data);

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
	}
}