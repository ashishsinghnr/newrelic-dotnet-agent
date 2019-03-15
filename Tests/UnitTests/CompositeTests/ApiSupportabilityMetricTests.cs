﻿using NewRelic.Agent.Core;
using NewRelic.Agent.Core.Api;
using NewRelic.Agent.Core.Metric;
using NewRelic.Agent.Extensions.Providers.Wrapper;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace CompositeTests
{
	[TestFixture]
	public class ApiSupportabilityMetricTests
	{
		private const string SupportabilityMetricPrefix = "Supportability/ApiInvocation/";

		private static CompositeTestAgent _compositeTestAgent;
		private IApiSupportabilityMetricCounters _apiSupportabilityMetricCounters;
		private IAgentWrapperApi _agentWrapperApi;

		[SetUp]
		public void SetUp()
		{
			_compositeTestAgent = new CompositeTestAgent();
			_agentWrapperApi = _compositeTestAgent.GetAgentWrapperApi();
			_apiSupportabilityMetricCounters = _compositeTestAgent.Container.Resolve<IApiSupportabilityMetricCounters>();
		}

		[TearDown]
		public static void TearDown()
		{
			_compositeTestAgent.Dispose();
		}

		[Test]
		public void CreateDistributedTracePayloadTest()
		{
			CallTransactionApiBridgeMethod(transactionBridgeApi => transactionBridgeApi.CreateDistributedTracePayload(), "CreateDistributedTracePayload");
		}

		[Test]
		public void AcceptDistributedTracePayloadTest()
		{
			CallTransactionApiBridgeMethod(transactionBridgeApi => transactionBridgeApi.AcceptDistributedTracePayload("testString", 0), "AcceptDistributedTracePayload");
		}

		[Test]
		public void CurrentTransactionTest()
		{
			var agentWrapperApi = _compositeTestAgent.GetAgentWrapperApi();
			var agentBridgeApi = new AgentBridgeApi(agentWrapperApi, _apiSupportabilityMetricCounters);

			var currentTransaction = agentBridgeApi.CurrentTransaction;

			HarvestAndValidateMetric("CurrentTransaction");
		}

		[Test]
		public void DisableBrowserMonitoring()
		{
			AgentApi.DisableBrowserMonitoring();
			HarvestAndValidateMetric("DisableBrowserMonitoring");
		}

		[Test]
		public void AddCustomParameter()
		{
			CallAgentApiMethodRequiringTransaction(() => AgentApi.AddCustomParameter("customParameter", "1234"), "AddCustomParameter");
		}

		[Test]
		public void GetBrowserTimingHeader()
		{
			CallAgentApiMethodRequiringTransaction(() => AgentApi.GetBrowserTimingHeader(), "GetBrowserTimingHeader");
		}

		[Test]
		public void GetBrowserTimingFooter()
		{
			CallAgentApiMethodRequiringTransaction(() => AgentApi.GetBrowserTimingFooter(), "GetBrowserTimingFooter");
		}

		[Test]
		public void IgnoreApdex()
		{
			CallAgentApiMethodRequiringTransaction(() => AgentApi.IgnoreApdex(), "IgnoreApdex");
		}

		[Test]
		public void IgnoreTransaction()
		{
			CallAgentApiMethodRequiringTransaction(() => AgentApi.IgnoreTransaction(), "IgnoreTransaction");
		}

		[Test]
		public void IncrementCounter()
		{
			CallAgentApiMethodRequiringTransaction(() => AgentApi.IncrementCounter("fred"), "IncrementCounter");
		}

		[Test]
		public void NoticeError()
		{
			var exception = new IndexOutOfRangeException();
			CallAgentApiMethodRequiringTransaction(() => AgentApi.NoticeError(exception), "NoticeError");
		}

		[Test]
		public void RecordCustomEvent()
		{
			CallAgentApiMethodRequiringTransaction(() => AgentApi.RecordCustomEvent("customEvent", null), "RecordCustomEvent");
		}

		[Test]
		public void RecordMetric()
		{
			CallAgentApiMethodRequiringTransaction(() => AgentApi.RecordMetric("metricName", 1f), "RecordMetric");
		}

		[Test]
		public void RecordResponseTimeMetric()
		{
			CallAgentApiMethodRequiringTransaction(() => AgentApi.RecordResponseTimeMetric("responseTimeMetric", 1234L), "RecordResponseTimeMetric");
		}

		[Test]
		public void SetApplicationName()
		{
			CallAgentApiMethodRequiringTransaction(() => AgentApi.SetApplicationName("applicationName"), "SetApplicationName");
		}

		[Test]
		public void SetTransactionName()
		{
			CallAgentApiMethodRequiringTransaction(() => AgentApi.SetTransactionName("custom", "transactionName"), "SetTransactionName");
		}

		[Test]
		public void SetUserParameters()
		{
			CallAgentApiMethodRequiringTransaction(() => AgentApi.SetUserParameters("userName", "accountName", "productName"), "SetUserParameters");
		}

		[Test]
		public void StartAgent()
		{
			CallAgentApiMethodRequiringTransaction(() => AgentApi.StartAgent(), "StartAgent");
		}

		private void CallAgentApiMethodRequiringTransaction(Action apiMethod, string expectedMetricName)
		{
			using(var transaction = _agentWrapperApi.CreateWebTransaction(WebTransactionType.ASP, "TransactionName"))
			{
				apiMethod();
			}

			HarvestAndValidateMetric(expectedMetricName);
		}

		private void HarvestAndValidateMetric(string expectedMetricName)
		{
			_compositeTestAgent.Harvest();

			// ASSERT
			var expectedMetrics = new List<ExpectedMetric>
			{
				new ExpectedCountMetric {Name =  SupportabilityMetricPrefix + expectedMetricName, CallCount = 1}
			};

			MetricAssertions.MetricsExist(expectedMetrics, _compositeTestAgent.Metrics);
		}

		private void CallTransactionApiBridgeMethod(Action<TransactionBridgeApi> apiMethod, string expectedMetricName)
		{
			using (var transaction = _agentWrapperApi.CreateWebTransaction(WebTransactionType.ASP, "TransactionName"))
			{
				var transactionBridgeApi = new TransactionBridgeApi(transaction, _apiSupportabilityMetricCounters);
				var segment = _compositeTestAgent.GetAgentWrapperApi().StartTransactionSegmentOrThrow("segment");

				apiMethod(transactionBridgeApi);

				segment.End();
			}

			HarvestAndValidateMetric(expectedMetricName);
		}
	}
}