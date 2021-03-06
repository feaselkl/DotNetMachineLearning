﻿using System;
using System.IO;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace DotNetMachineLearning.BillsNaiveBayes
{
	public class BillsModelTrainer
	{
		public IDataView GetRawData(MLContext mlContext, string inputPath)
		{
			return mlContext.Data.LoadFromTextFile<RawInput>(path: inputPath, hasHeader: true, separatorChar: ',');
		}

		public EstimatorChain<Microsoft.ML.Transforms.KeyToValueMappingTransformer> GetPipeline(
			MLContext mlContext, IEstimator<ITransformer> trainer)
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
				.Append(trainer)/*mlContext.MulticlassClassification.Trainers.NaiveBayes(
					labelColumnName: "Label", featureColumnName: "Features"))*/
				// L-BFGS is awful
				//.Append(mlContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy(labelColumnName: "Label", featureColumnName: "Features"))
				// Stochastic DCA is good but SLOW
				//.Append(mlContext.MulticlassClassification.Trainers.SdcaNonCalibrated(labelColumnName: "Label", featureColumnName: "Features"))
				.Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedOutcome", "PredictedLabel"));

			return pipeline;
		}

		public TransformerChain<Microsoft.ML.Transforms.KeyToValueMappingTransformer> TrainModel(
			MLContext mlContext, IDataView data, IEstimator<ITransformer> trainer)
		{
			var pipeline = GetPipeline(mlContext, trainer);
			var model = pipeline.Fit(data);

			return model;
		}

		public void SaveModel(MLContext mlContext, ITransformer model, string modelPath)
		{
			using (var stream = File.Create(modelPath))
			{
				mlContext.Model.Save(model, null, stream);
			}
		}

		public ITransformer LoadModel(MLContext mlContext, string modelPath)
		{
			ITransformer loadedModel;
			using (var stream = File.OpenRead(modelPath))
			{
				DataViewSchema dvs;
				loadedModel = mlContext.Model.Load(stream, out dvs);
			}

			return loadedModel;
		}
	}
}
