using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DotNetMachineLearning.Models;
using DotNetMachineLearning.BillsNaiveBayes;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace DotNetMachineLearning.Controllers
{
	public class BillsController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}

		[HttpPost, ValidateAntiForgeryToken]
		public IActionResult Index(BillsViewModel bvm)
		{
			MLContext mlContext = new MLContext(seed: 9997);
			BillsModelTrainer bmt = new BillsModelTrainer();

			var data = bmt.GetRawData(mlContext, "2018Bills.csv");
			var model = bmt.TrainModel(mlContext, data);

			PredictionEngineBase<RawInput, Prediction> predictor = model.CreatePredictionEngine<RawInput, Prediction>(mlContext);
			var outcome = predictor.Predict(new RawInput
			{
				Game = 0,
				Quarterback = bvm.Quarterback,
				Location = bvm.Location.ToString(),
				NumberOfPointsScored = bvm.NumberOfPointsScored,
				TopReceiver = bvm.TopReceiver,
				TopRunner = bvm.TopRunner,
				NumberOfSacks = 0,
				NumberOfDefensiveTurnovers = 0,
				MinutesPossession = 0,
				Outcome = "WHO KNOWS?"
			});

			return Content($"Under these conditions, the most likely outcome is a {outcome.Outcome.ToLower()}.");
		}
	}
}