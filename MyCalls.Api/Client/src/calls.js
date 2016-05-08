import { BindingEngine, inject } from 'aurelia-framework';
import { HttpClient, json } from 'aurelia-fetch-client';
import moment  from 'moment';

@inject(HttpClient, BindingEngine)
export class Welcome {
  heading = 'Calls';

  constructor(http, bindingEngine) {

    this.baseAddress = "http://mycalls.azurewebsites.net/api/";

    this.http = http;
    this.bindingEngine = bindingEngine;
    
    this.isLoading = false;
    
    this.customFilterList = [];

    this.selectedCustomFilter = null;

    this.filterPropertyInfoList = [];
    this.selectedFilterPropertyInfo = null;
    this.selectedCondition = 0;
    this.selectedCompareMode = 1;
    this.filterValue = null;

    this.selectedFilters = [];

    this.customFilterName = null;
    this.calls = [];
    //this.configHttp();

    this.subscriptions = [];

    let subFpi = this.bindingEngine
      .propertyObserver(this, 'selectedFilterPropertyInfo')
      .subscribe(this.selectedFilterPropertyInfoChanged);

    this.subscriptions.push(subFpi);


    let subCf = this.bindingEngine
      .propertyObserver(this, 'selectedCustomFilter')
      .subscribe((newValue) => {
        this.customFilterName = newValue ? newValue.Name : null;
        this.selectedFilters = newValue ? newValue.Properties : [];
        this.loadCalls(this.selectedFilters);
      });

    this.subscriptions.push(subCf);
  }


  selectedFilterPropertyInfoChanged(newValue) {

  }



  activate() {
    this.loadCustomFilters();
    this.loadFilterProperties();
    this.loadCalls();
  }

  loadCustomFilters() {
      this.http.fetch(this.baseAddress+'calls/CustomFilters', { method: 'GET' })
      .then(this.parseJson)
      .then(data => {
        this.customFilterList = data;
      });
  }


  loadFilterProperties() {
      this.http.fetch(this.baseAddress+'calls/FilterPropertyInfos', { method: 'GET' })
      .then(this.parseJson)
      .then(data => {
        this.filterPropertyInfoList = data;
      });
  }

  loadCalls(filters) {
    this.isLoading = true;
    this.http.fetch(this.baseAddress+'calls/data', {
      method: 'POST',
      body: json(filters)
    })
      .then(this.parseJson)
      .then(data => {
        this.isLoading = false;
        this.calls = data;
      });
  }


  addFilter() {

    //the first filter conditon should always be AND
    let condition = (this.selectedFilters.length ? this.selectedCondition : 0);
    let compareMode = (this.selectedFilterPropertyInfo.FilterType === 3 ? 3 : (this.selectedFilterPropertyInfo.FilterType === 1 ? this.selectedCompareMode : 0));

    let filter = {
      Info: this.selectedFilterPropertyInfo,
      Condition: condition,
      CompareMode: compareMode,
      Value: this.filterValue
    };

    this.selectedFilters.push(filter);

    this.filterValue = null;
    //this.selectedFilterPropertyInfo = this.filterPropertyInfoList[0];

    this.loadCalls(this.selectedFilters);
  }

  deleteFilter(filter) {
    let idx = this.selectedFilters.indexOf(filter);
    this.selectedFilters.splice(idx, 1);

    if (this.selectedFilters.length === 0) {
        this.http.fetch(this.baseAddress+'calls/deleteCustomFilter/' + this.selectedCustomFilter.Name, { method: 'DELETE' })
        .then(t => this.loadCustomFilters());
    }

    this.loadCalls(this.selectedFilters);
  }


  saveFilter() {

    let filterParam = {
      Name: this.customFilterName,
      Properties: this.selectedFilters
    };

    this.http.fetch(this.baseAddress+'calls/saveCustomFilter', {
      method: 'POST',
      body: json(filterParam)
    })
      .then(this.parseJson)
      .then(data => {        
      }).then(t => this.loadCustomFilters());
  }



  parseJson(response) {
    return response.json();
  }


  deactivate() {
    this.subscriptions.forEach(function (sub) {
      sub.dispose();
    });
  }


  configHttp() {
    this.http.configure(config => {
      config
        .withBaseUrl('http://mycalls.azurewebsites.net/api/')
        .withDefaults({
          headers: {
            'Accept': 'application/json',
            'X-Requested-With': 'Fetch'
          }
        })
        .withInterceptor({
          request(request) {
            console.log(`Requesting ${request.method} ${request.url}`);
            return request;
          },
          response(response) {
            console.log(`Received ${response.status} ${response.url}`);
            return response;
          }
        });
    });
  }
}


export class SecondsToTimeValueConverter {
  toView(value) {
    let sec_num = parseInt(value, 10);
    let hours = Math.floor(sec_num / 3600);
    let minutes = Math.floor((sec_num - (hours * 3600)) / 60);
    let seconds = sec_num - (hours * 3600) - (minutes * 60);

    if (hours < 10) { hours = '0' + hours; }
    if (minutes < 10) { minutes = '0' + minutes; }
    if (seconds < 10) { seconds = '0' + seconds; }
    return hours + ':' + minutes + ':' + seconds;
  }
}

export class CompareModeValueConverter {
  toView(value) {
    let result = '';
    switch (value) {
      case 0:
        result = 'Equals';
        break;
      case 1:
        result = 'Greater than';
        break;
      case 2:
        result = 'Lesser then';
        break;
      case 3:
        result = 'Contains';
        break;
      case 4:
        result = 'Like';
        break;
      default:
        result = '';
    }

    return result;
  }
}
