﻿using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RP1AnalyticsWebApp.Models;
using RP1AnalyticsWebApp.Services;

namespace RP1AnalyticsWebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CareerLogsController : ControllerBase
    {
        private readonly CareerLogService _careerLogService;
        private readonly TelemetryClient _telemetry;

        private bool IsLocalhost => string.Equals(HttpContext.Request.Host.Host, "localhost", System.StringComparison.OrdinalIgnoreCase);

        public CareerLogsController(CareerLogService careerLogService, TelemetryClient telemetry)
        {
            _careerLogService = careerLogService;
            _telemetry = telemetry;
        }

        [HttpGet(Name = "GetCareerLogs")]
        public ActionResult<List<CareerLog>> GetCareerLogs()
        {
            _telemetry.TrackEvent("CareerLogsController-GetCareerLogs");
            var res = _careerLogService.Get();
            if (!IsLocalhost)
            {
                res.ForEach(c => c.RemoveNonPublicData());
            }
            return res;
        }

        [HttpGet("List", Name = "GetCareerList")]
        public ActionResult<List<CareerListItem>> GetCareerList(string userName = null)
        {
            _telemetry.TrackEvent("CareerLogsController-GetCareerList");
            return _careerLogService.GetCareerList(userName);
        }

        [HttpGet("{id:length(24)}", Name = "GetCareerLog")]
        public ActionResult<CareerLog> GetCareerLog(string id)
        {
            _telemetry.TrackEvent("CareerLogsController-GetCareerLog", new Dictionary<string, string>
            {
                { nameof(id), id }
            });

            var careerLog = _careerLogService.Get(id);
            if (careerLog == null)
            {
                return NotFound();
            }

            if (!IsLocalhost)
            {
                careerLog.RemoveNonPublicData();
            }

            return careerLog;
        }

        [HttpGet("{id:length(24)}/Contracts", Name = "GetCareerContracts")]
        public ActionResult<List<ContractEvent>> GetCareerContracts(string id)
        {
            _telemetry.TrackEvent("CareerLogsController-GetCareerContracts", new Dictionary<string, string>
            {
                { nameof(id), id }
            });

            List<ContractEvent> contractEvents = _careerLogService.GetContractsForCareer(id);
            if (contractEvents == null)
            {
                return NotFound();
            }

            return contractEvents;
        }

        [HttpGet("{id:length(24)}/CompletedMilestones", Name = "GetCareerCompletedMilestones")]
        public ActionResult<List<ContractEvent>> GetCareerCompletedMilestones(string id)
        {
            _telemetry.TrackEvent("CareerLogsController-GetCareerCompletedMilestones", new Dictionary<string, string>
            {
                { nameof(id), id }
            });

            List<ContractEvent> contractEvents = _careerLogService.GetCompletedMilestonesForCareer(id);
            if (contractEvents == null)
            {
                return NotFound();
            }

            return contractEvents;
        }

        [HttpGet("Contracts", Name = "GetRecords")]
        public ActionResult<List<ContractEventWithCareerInfo>> GetRecords()
        {
            _telemetry.TrackEvent("CareerLogsController-GetRecords");

            List<ContractEventWithCareerInfo> events = _careerLogService.GetRecords();
            return events;
        }

        [HttpGet("Contracts/{contract}", Name = "GetCompletionsForContract")]
        public ActionResult<List<ContractEventWithCareerInfo>> GetCompletionsForContract(string contract)
        {
            _telemetry.TrackEvent("CareerLogsController-GetCompletionsForContract", new Dictionary<string, string>
            {
                { nameof(contract), contract }
            });

            List<ContractEventWithCareerInfo> events = _careerLogService.GetEventsForContract(contract, ContractEventType.Complete);
            return events;
        }

        [HttpPost(Name = "CreateCareer")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public ActionResult<CareerLog> CreateCareer(CareerLog log)
        {
            _telemetry.TrackEvent("CareerLogsController-CreateCareer", new Dictionary<string, string>
            {
                { nameof(CareerLog.name), log.name }
            });

            log.userLogin = User.Identity.Name;

            CareerLog res = _careerLogService.Create(log);
            return CreatedAtRoute("CreateCareer", res);
        }

        [HttpPatch("{token:length(32)}", Name = "UpdateCareer")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<CareerLogDto> UpdateCareer(string token, CareerLogDto careerLog)
        {
            _telemetry.TrackEvent("CareerLogsController-UpdateCareer", new Dictionary<string, string>
            {
                { nameof(token), token }
            });

            CareerLog res = _careerLogService.Update(token, careerLog);
            return CreatedAtRoute("UpdateCareer", res);
        }
    }
}
