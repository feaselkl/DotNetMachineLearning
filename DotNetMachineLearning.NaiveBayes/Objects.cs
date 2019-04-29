using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetMachineLearning.BillsNaiveBayes
{
	public class RawInput
	{
		//Game,QB,HomeAway,NumPointsScored,TopReceiver,TopRunner,NumSacks,NumDefTurnovers,MinutesPossession,Outcome
		//1,Peterman,Away,3,Zay Jones,Marcus Murphy,1,1,25,Loss

		[LoadColumn(0)]
		public float Game { get; set; }
		[LoadColumn(1)]
		public string Quarterback { get; set; }
		[LoadColumn(2)]
		public string Location { get; set; }
		[LoadColumn(3)]
		public float NumberOfPointsScored { get; set; }
		[LoadColumn(4)]
		public string TopReceiver { get; set; }
		[LoadColumn(5)]
		public string TopRunner { get; set; }
		[LoadColumn(6)]
		public float NumberOfSacks { get; set; }
		[LoadColumn(7)]
		public float NumberOfDefensiveTurnovers { get; set; }
		[LoadColumn(8)]
		public float MinutesPossession { get; set; }
		[LoadColumn(9)]
		[ColumnName("Label")]
		public string Outcome { get; set; }
	}

	public class Prediction
	{
		[ColumnName("PredictedOutcome")]
		public string Outcome { get; set; }
	}

	public class BinaryPrediction
	{
		[ColumnName("PredictedLabel")]
		public bool WillBeVictorious { get; set; }
	}
}
