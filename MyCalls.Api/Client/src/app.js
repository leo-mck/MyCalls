export class App {
  configureRouter(config, router) {
    config.title = 'MyCalls';
    config.map([
      { route: ['', 'calls'], name: 'calls',      moduleId: 'calls',      nav: true, title: 'Calls' }
      //{ route: 'About',         name: 'about',        moduleId: 'about',        nav: true, title: 'About' }
    ]);

    this.router = router;
  }
}
