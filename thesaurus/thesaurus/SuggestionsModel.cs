﻿using System;
using Dynamo.ViewModels;
using Dynamo.Graph.Nodes;
using System.Reflection;
using Dynamo.Models;
using Accord.Statistics.Models.Markov;
using Accord.Statistics.Filters;
using Accord.IO;
using Accord.MachineLearning.Bayes;

namespace thesaurus
{
    public class SuggestionsModel
    {
        public DynamoViewModel DynamoViewModel { get; set; }
        private object _loadedModel;
        private Codification _loadedCodebook;

        public SuggestionsModel(DynamoViewModel dvm)
        {
            DynamoViewModel = dvm;
            LoadModel();
        }

        /// <summary>
        /// This handler responds to clicking on the SuggestionNodeButton and create node to the Dynamo
        /// session current workspace
        /// </summary>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        public bool PlaceNode(string nodeName)
        {
            // Get Reference of DynamoModel
            var dm = DynamoViewModel.Model;
            var nsm = dm.SearchModel;

            foreach (var se in nsm.SearchEntries)
            {
                if (se.FullName.EndsWith(nodeName, StringComparison.OrdinalIgnoreCase) || se.CreationName.EndsWith(nodeName, StringComparison.OrdinalIgnoreCase))
                {
                    var dynMethod = se.GetType().GetMethod("ConstructNewNodeModel",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    var obj = dynMethod.Invoke(se, new object[] { });
                    var nM = obj as NodeModel;

                    try
                    {
                        dm.ExecuteCommand(new DynamoModel.CreateNodeCommand(nM, 0, 0, true, false));
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        private void LoadModel()
        {
            switch (TrainModel.TrainingMode)
            {
                case "bayes":
                    _loadedModel = Serializer.Load<NaiveBayes>("thesaurus_bayes.accord");
                    _loadedCodebook = Serializer.Load<Codification>("thesaurus_codebook.accord");
                    break;
                case "markov":
                    _loadedModel = Serializer.Load<HiddenMarkovModel>("thesaurus_HMModel.accord");
                    _loadedCodebook = Serializer.Load<Codification>("thesaurus_codebook.accord");
                    break;
            }
        }

        public string[] Predict(string nodeName)
        {
            try
            {
                switch (TrainModel.TrainingMode)
                {
                    case "bayes":
                        var bayesModel = _loadedModel as NaiveBayes;
                        var instance = _loadedCodebook.Transform(nodeName);
                        var c = bayesModel.Decide(instance);
                        var result = _loadedCodebook.Revert("output", c);

                        return new[] { result };
                    case "markov":
                        var markovModel = _loadedModel as HiddenMarkovModel;
                        var code = _loadedCodebook.Transform("Nodes", nodeName);
                        var predictSample = markovModel.Predict(new[] { code }, 1);
                        var predictResult = _loadedCodebook.Revert("Nodes", predictSample);
                        return predictResult;
                }

                return new string[] { };
            }
            catch
            {
                return new string[] { };
            }
        }
    }
}