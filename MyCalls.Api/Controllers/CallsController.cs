using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MyCalls.Data;
using System.Linq.Dynamic;
using System.Web.Http.Cors;
using Newtonsoft.Json;

namespace MyCalls.Api.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api/calls")]
    public class CallsController : ApiController
    {
        AppDbContext _ctx;
        static List<FilterPropertyInfo> _filterPropertyNames;


        public CallsController()
        {
            _ctx = new AppDbContext();

            var tags = _ctx.Tags.ToList();


            //this is the properties we allow the user use to filter the Calls dataset            
            _filterPropertyNames = new List<FilterPropertyInfo>()
            {                
                new FilterPropertyInfo()
                {
                    PropertyName = "Caller.Name", Name = "Caller Name", FilterType = FilterPropertyType.String
                },
                new FilterPropertyInfo()
                {
                    PropertyName = "Callee.Name", Name = "Callee Name", FilterType = FilterPropertyType.String
                },
                new FilterPropertyInfo()
                {
                    PropertyName = "Callee.Number", Name = "Callee Number", FilterType = FilterPropertyType.String
                },
                new FilterPropertyInfo()
                {
                    PropertyName = "DurationSeconds", Name = "Call Duration (seconds)", FilterType = FilterPropertyType.Number
                },
                new FilterPropertyInfo()
                {
                    PropertyName = "Caller.Tags.Name", Name = "Caller Tag", FilterType = FilterPropertyType.List, Data = tags
                },
                new FilterPropertyInfo()
                {
                    PropertyName = "Callee.Tags.Name", Name = "Callee Tag", FilterType = FilterPropertyType.List, Data = tags
                }
            };
        }

        [Route("FilterPropertyInfos")]
        [HttpGet]
        public IEnumerable<FilterPropertyInfo> FilterPropertyInfo()
        {
            return _filterPropertyNames;
        }


        [Route("data")]
        public IEnumerable<Call> LoadData(IEnumerable<FilterProperty> filterProperties)
        {
            var callsQuery = _ctx.Calls.Include("Caller.Tags").Include("Callee.Tags").AsQueryable();

            if (filterProperties != null)
            {
                var queryFilter = new QueryFilter();
                queryFilter.Properties.AddRange(filterProperties);
                callsQuery = callsQuery.Where(queryFilter.GenerateFilter());
            }

            return callsQuery;
        }


        [Route("CustomFilters")]
        [HttpGet]
        public IEnumerable<CustomFilterParams> GetCustomFilters()
        {
            return
                _ctx.CustomFilters.ToList().Select(
                    x =>
                        new CustomFilterParams()
                        {
                            Name = x.Name,
                            Properties = JsonConvert.DeserializeObject<FilterProperty[]>(x.FilterJson)
                        });
        }


        [Route("deleteCustomFilter/{filterName}")]
        [HttpDelete]
        public bool DeleteCustomFilter(string filterName)
        {
            var customFilter = _ctx.CustomFilters.FirstOrDefault(x => x.Name == filterName);
            _ctx.CustomFilters.Remove(customFilter);
            return _ctx.SaveChanges() == 1;
        }


        [Route("saveCustomFilter")]
        [HttpPost]
        public bool SaveCustomFilter(CustomFilterParams filterParams)
        {
            var customFilter = _ctx.CustomFilters.FirstOrDefault(x => x.Name == filterParams.Name);

            var filterJson = JsonConvert.SerializeObject(filterParams.Properties);

            if (customFilter == null)
            {
                customFilter = new CustomFilter() {FilterJson = filterJson, Name = filterParams.Name};
                _ctx.CustomFilters.Add(customFilter);
            }
            else
            {
                customFilter.FilterJson = filterJson;
            }

            return _ctx.SaveChanges() == 1;
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _ctx.Dispose();
            }
            base.Dispose(disposing);
        }
    }


    public class CustomFilterParams
    {
        public string Name { get; set; }
        public IEnumerable<FilterProperty> Properties { get; set; }
    }
}
