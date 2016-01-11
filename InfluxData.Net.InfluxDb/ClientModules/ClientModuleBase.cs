﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using InfluxData.Net.InfluxDb.Infrastructure;
using InfluxData.Net.InfluxDb.RequestClients;
using System.Threading.Tasks;
using InfluxData.Net.Common.Helpers;
using InfluxData.Net.InfluxDb.Models.Responses;

namespace InfluxData.Net.InfluxDb.ClientModules
{
    public class ClientModuleBase
    {
        protected IInfluxDbRequestClient RequestClient { get; private set; }

        public ClientModuleBase(IInfluxDbRequestClient requestClient)
        {
            this.RequestClient = requestClient;
        }

        protected async Task<IInfluxDbApiResponse> GetQueryAsync(string query)
        {
            var requestParams = RequestParamsBuilder.BuildQueryRequestParams(query);
            var response = await this.RequestClient.GetQueryAsync(requestParams);

            return response;
        }

        protected async Task<IInfluxDbApiResponse> GetQueryAsync(string dbName, string query)
        {
            var requestParams = RequestParamsBuilder.BuildQueryRequestParams(dbName, query);
            var response = await this.RequestClient.GetQueryAsync(requestParams);

            return response;
        }

        protected QueryResponse ReadAsQueryResponse(IInfluxDbApiResponse response)
        {
            var queryResponse = response.ReadAs<QueryResponse>();

            Validate.NotNull(queryResponse, "queryResponse");
            Validate.NotNull(queryResponse.Results, "queryResponse.Results");

            if (!String.IsNullOrEmpty(queryResponse.Error))
            {
                throw new InfluxDbApiException(HttpStatusCode.BadRequest, queryResponse.Error);
            }

            // Apparently a 200 OK can return an error in the results
            // https://github.com/influxdb/influxdb/pull/1813
            var erroredResults = queryResponse.Results.Where(result => !String.IsNullOrEmpty(result.Error));
            foreach (var erroredResult in erroredResults)
            {
                throw new InfluxDbApiException(HttpStatusCode.BadRequest, erroredResult.Error);
            }

            return queryResponse;
        }

        protected void ValidateQueryResponse(IInfluxDbApiResponse response)
        {
            ReadAsQueryResponse(response);
        }

        protected IEnumerable<Serie> GetSeries(SeriesResult result)
        {
            Validate.NotNull(result, "result");
            return result.Series != null ? result.Series.ToList() : new List<Serie>();
        }
    }
}
