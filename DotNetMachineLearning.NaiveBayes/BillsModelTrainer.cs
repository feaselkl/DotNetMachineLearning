using System;
using System.IO;
using Microsoft.Data.DataView;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;

namespace DotNetMachineLearning.BillsNaiveBayes
{
	public class BillsModelTrainer
	{
		public IDataView GetRawData(MLContext mlContext, string inputPath)
		{
			return mlContext.Data.LoadFromTextFile<RawInput>(path: inputPath, hasHeader: true, separatorChar: ',');
		}

		public TransformerChain<Microsoft.ML.Transforms.KeyToValueMappingTransformer> TrainModel(
			MLContext mlContext, IDataView data)
		{
			var pipeline =
				mlContext.Transforms.CustomMapping<QBInputRow, QBOutputRow>(
					QBCustomMappings.QBMapping, nameof(QBCustomMappings.QBMapping))
				.Append(mlContext.Transforms.CustomMapping<PointsInputRow, PointsOutputRow>(
					PointsCustomMappings.PointsMapping, nameof(PointsCustomMappings.PointsMapping)))
				// We could potentially use these features for a different model like a fast forest.
				.Append(mlContext.Transforms.DropColumns(new[] { "NumberOfSacks", "NumberOfDefensiveTurnovers",
					"MinutesPossession" }))
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
				// Naive Bayes is pretty good
				.Append(mlContext.MulticlassClassification.Trainers.NaiveBayes(
					labelColumnName: "Label", featureColumnName: "Features"))
				// Logistic regression is awful
				//.Append(mlContext.MulticlassClassification.Trainers.LogisticRegression(labelColumnName: "Label", featureColumnName: "Features"))
				// Stochastic DCA is good but SLOW
				//.Append(mlContext.MulticlassClassification.Trainers.StochasticDualCoordinateAscent(labelColumnName: "Label", featureColumnName: "Features"))
				.Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedOutcome", "PredictedLabel"));

			var model = pipeline.Fit(data);

			return model;
		}

		public void SaveModel(MLContext mlContext, ITransformer model, string modelPath)
		{
			using (var stream = File.Create(modelPath))
			{
				mlContext.Model.Save(model, stream);
			}
		}

		public ITransformer LoadModel(MLContext mlContext, string modelPath)
		{
			ITransformer loadedModel;
			using (var stream = File.OpenRead(modelPath))
			{
				loadedModel = mlContext.Model.Load(stream);
			}

			return loadedModel;
		}
	}
}
