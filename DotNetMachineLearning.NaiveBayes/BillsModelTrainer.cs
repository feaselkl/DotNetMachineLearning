using System;
using System.IO;
using Microsoft.Data.DataView;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace DotNetMachineLearning.BillsNaiveBayes
{
	public class BillsModelTrainer
	{
		// These are helper classes for data transformation.
		class QBInputRow
		{
			public string Quarterback { get; set; }
		}
		class QBOutputRow
		{
			public string QuarterbackName { get; set; }
		}

		class PointsInputRow
		{
			public float NumberOfPointsScored { get; set; }
		}

		class PointsOutputRow
		{
			public bool DoubleDigitPoints { get; set; }
		}

		public IDataView GetRawData(MLContext mlContext, string inputPath)
		{
			return mlContext.Data.LoadFromTextFile<RawInput>(path: inputPath, hasHeader: true, separatorChar: ',');
		}

		public TransformerChain<Microsoft.ML.Transforms.KeyToValueMappingTransformer> TrainModel(MLContext mlContext, IDataView data)
		{
			Action<QBInputRow, QBOutputRow> qbmapping = (input, output) => output.QuarterbackName =
				(input.Quarterback == "Josh Allen") ? "Josh Allen" : "Nate Barkerson";
			Action<PointsInputRow, PointsOutputRow> pointsmapping = (input, output) => output.DoubleDigitPoints =
				(input.NumberOfPointsScored >= 10);

			var pipeline =
				mlContext.Transforms.CustomMapping(qbmapping, null)
				.Append(mlContext.Transforms.CustomMapping(pointsmapping, null))
				// We could potentially use these features for a different model like a fast forest.
				.Append(mlContext.Transforms.DropColumns(new[] { "NumberOfSacks", "NumberOfDefensiveTurnovers", "MinutesPossession" }))
				.Append(mlContext.Transforms.DropColumns(new[] { "Game", "Quarterback" }))
				.Append(mlContext.Transforms.Concatenate("FeaturesText", new[]
				{
					"QuarterbackName",
					"Location",
					"TopReceiver",
					"TopRunner"
				}))
				.Append(mlContext.Transforms.Text.FeaturizeText("Features", "FeaturesText"))
				// Label is text so it needs to be mapped to a key
				.Append(mlContext.Transforms.Conversion.MapValueToKey("Label"), TransformerScope.TrainTest)
				// Naive Bayes is prety good
				.Append(mlContext.MulticlassClassification.Trainers.NaiveBayes(labelColumnName: "Label", featureColumnName: "Features"))
				// Logistic regression is awful
				//.Append(mlContext.MulticlassClassification.Trainers.LogisticRegression(labelColumnName: "Label", featureColumnName: "Features"))
				// Stochastic DCA is good but SLOW
				//.Append(mlContext.MulticlassClassification.Trainers.StochasticDualCoordinateAscent(labelColumnName: "Label", featureColumnName: "Features"))
				.Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedOutcome", "PredictedLabel"));

			var model = pipeline.Fit(data);
			return model;
		}
	}
}
