//*****************************************************************************************
//*                                                                                       *
//* This is an auto-generated file by Microsoft ML.NET CLI (Command-Line Interface) tool. *
//*                                                                                       *
//*****************************************************************************************

using Microsoft.ML.Data;

namespace DotNetMachineLearningML.Model.DataModels
{
    public class ModelInput
    {
        [ColumnName("airline_sentiment"), LoadColumn(0)]
        public bool Airline_sentiment { get; set; }


        [ColumnName("text"), LoadColumn(1)]
        public string Text { get; set; }


    }
}
