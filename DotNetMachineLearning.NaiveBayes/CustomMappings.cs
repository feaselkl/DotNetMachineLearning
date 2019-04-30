using Microsoft.ML.Transforms;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetMachineLearning.BillsNaiveBayes
{
	/// <summary>
	/// A class which contains the custom mapping functionality that we need for our model.
	/// 
	/// It has a <see cref="CustomMappingFactoryAttributeAttribute"/> on it and
	/// derives from <see cref="CustomMappingFactory{TSrc, TDst}"/>.
	/// </summary>
	[CustomMappingFactoryAttribute(nameof(QBCustomMappings.QBMapping))]
	public class QBCustomMappings : CustomMappingFactory<QBInputRow, QBOutputRow>
	{
		// This is the custom mapping. We now separate it into a method, so that we can use it both in training and in loading.
		public static void QBMapping(QBInputRow input, QBOutputRow output) => output.QuarterbackName =
			(input.Quarterback == "Josh Allen") ? "Josh Allen" : "Nate Barkerson";

		// This factory method will be called when loading the model to get the mapping operation.
		public override Action<QBInputRow, QBOutputRow> GetMapping()
		{
			return QBMapping;
		}
	}

	/// <summary>
	/// A class which contains the custom mapping functionality that we need for our model.
	/// 
	/// It has a <see cref="CustomMappingFactoryAttributeAttribute"/> on it and
	/// derives from <see cref="CustomMappingFactory{TSrc, TDst}"/>.
	/// </summary>
	[CustomMappingFactoryAttribute(nameof(PointsCustomMappings.PointsMapping))]
	public class PointsCustomMappings : CustomMappingFactory<PointsInputRow, PointsOutputRow>
	{
		// This is the custom mapping. We now separate it into a method, so that we can use it both in training and in loading.
		public static void PointsMapping(PointsInputRow input, PointsOutputRow output) =>
			output.DoubleDigitPoints = (input.NumberOfPointsScored >= 10);

		// This factory method will be called when loading the model to get the mapping operation.
		public override Action<PointsInputRow, PointsOutputRow> GetMapping()
		{
			return PointsMapping;
		}
	}

	// These are helper classes for data transformation.
	public class QBInputRow
	{
		public string Quarterback { get; set; }
	}

	public class QBOutputRow
	{
		public string QuarterbackName { get; set; }
	}

	public class PointsInputRow
	{
		public float NumberOfPointsScored { get; set; }
	}

	public class PointsOutputRow
	{
		public bool DoubleDigitPoints { get; set; }
	}
}
