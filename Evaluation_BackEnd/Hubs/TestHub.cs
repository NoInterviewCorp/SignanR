using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Evaluation_BackEnd.Models;
using Evaluation_BackEnd.Persistence;
using Learners.Services;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Evaluation_BackEnd.StaticData;
namespace asp_back.hubs {
    public class TestHub : Hub {
        private static Dictionary<string,string> testUsers;
        private ITestMethods methods;
        private QueueHandler queuehandler;
        private TemporaryData temp;
        private static Dictionary<string, TemporaryData> data;
        public TestHub (ITestMethods _methods, QueueHandler _queuehandler) {
            this.methods = _methods;
            this.queuehandler = _queuehandler;
        }
        public async Task newMessage (string username, string value) {
            await Clients.All.SendAsync ("messageReceived", username, value);
        }
        // public async Task GetAllTechnoligies () {
        //     var tech = methods.GetAllTechnologies ();
        //     await Clients.Caller.SendAsync ("GotAllTechnoligies", tech);
        // }
        public async Task RequestConcepts (string username, string technology) {
            methods.RequestConceptFromTechnology (username, technology);
            await Clients.Caller.SendAsync ("Request For Concept Recieved");
        }
        public async Task OnStart (string username, string tech, List<string> concepts) 
        {
            ConnectionData.userconnectiondata.Add(username,Context.ConnectionId);
            temp = new TemporaryData (tech, concepts);
            methods.OnStart (temp, username, tech);
            methods.GetQuestionsBatch (username, tech, concepts);
            await Clients.Caller.SendAsync ("Temporary Object Created");
        }
        public async Task OnFinish () {
            await Clients.Caller.SendAsync ("Data Seeded");
        }
        public async Task CountQuizAttempts (string tech, string username) {
            bool AttemptedEarlier = false;
            AttemptedEarlier = methods.CountQuizAttempts (tech, username);
            await Clients.Caller.SendAsync ("Got the Response", AttemptedEarlier);
        }
        public async Task EvaluateAnswer (string QuestionId, string OptionId) {
            await Clients.Caller.SendAsync ("Answer Evaluated");
        }
        public async Task GetQuestions (string username, string tech, string concept) {
            methods.GetQuestions (username, tech, concept);
            await Clients.Caller.SendAsync ("Got Questions");
        }
        public override async Task OnConnectedAsync () 
        {
            await base.OnConnectedAsync ();
        }

        public override async Task OnDisconnectedAsync (Exception exception) 
        {
            await base.OnDisconnectedAsync (exception);
        }
        
    }
}