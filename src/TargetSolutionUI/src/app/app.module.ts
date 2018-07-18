import { BrowserModule } from '@angular/platform-browser';
import { NgModule, APP_INITIALIZER } from '@angular/core';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';

import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { Http, HttpModule } from '@angular/http';

import { FilterPipe } from './shared/pipes/filter.pipe';
import { AppComponent } from './app.component';
import { AppConfig }  from './app.config';

import { MSALService } from './shared/services/msal.service';
import { AuthenticationHttpInterceptor } from './shared/services/authentication.httpInterceptor';
import { HeaderComponent } from './header/header.component';
import { FooterComponent } from './footer/footer.component';
import { PagenotfoundComponent } from './pagenotfound/pagenotfound.component';
import { HomeComponent } from './home/home.component';
import { AssetmanagementComponent } from './assetmanagement/assetmanagement.component'
import { LivetrackingComponent } from './livetracking/livetracking.component';
import { ReportsComponent } from './reports/reports.component';
import { AdminstrationComponent } from './adminstration/adminstration.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CreateTrailerService } from "./trailercompany/create-and-edit-trailer-data/createTrailer.service"
import { CreateDispatchOfficeService } from './dispatchoffice/create-and-update-dispatch-office/createDispatchOffice.service';
import { CreateUsersService } from './users/create-and-update-users/createUsers.service';

import { CreateAndEditTrailerDataComponent } from './trailercompany/create-and-edit-trailer-data/create-and-edit-trailer-data.component';
import { Routes, RouterModule } from '@angular/router';
import { DashboardComponent } from './dashboard/dashboard.component';
import { TrailercompanyComponent } from './trailercompany/trailercompany.component';
import { PagerService } from './tile/pager.service';
import { AccordionModule } from 'ngx-bootstrap/accordion';
import { ModalModule } from 'ngx-bootstrap/modal'
import { TimepickerModule } from 'ngx-bootstrap/timepicker';
import { PopoverModule } from 'ngx-bootstrap/popover';
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { ButtonsModule } from 'ngx-bootstrap/buttons';

import { AgmCoreModule, MapsAPILoader } from '@agm/core';
import { TileComponent } from './tile/tile.component';

import { ClearinputDirective } from './shared/directives/clearinput.directive';
import { DispatchofficeComponent } from './dispatchoffice/dispatchoffice.component';
import { LocationsComponent } from './locations/locations.component';
import { DriversComponent } from './drivers/drivers.component';
import { UsersComponent } from './users/users.component';
import { OrderModule } from 'ngx-order-pipe';
// import { ExcelService } from './shared/services/excel.service';
import { AlertsComponent } from './alerts/alerts.component';
import { AssetsComponent } from './assets/assets.component';
import { ViewtrailerdetailComponent } from './trailercompany/viewtrailerdetail/viewtrailerdetail.component';
import { ToastrModule } from 'ngx-toastr';
import { DisableControlDirectiveDirective } from './shared/directives/disable-control-directive.directive';
import { CreateAndUpdateDispatchOfficeComponent } from './dispatchoffice/create-and-update-dispatch-office/create-and-update-dispatch-office.component';
import { ViewDispatchOfficeComponent } from './dispatchoffice/view-dispatch-office/view-dispatch-office.component';
import { CreateAndUpdateUsersComponent } from './users/create-and-update-users/create-and-update-users.component';
import { ViewUsersComponent } from './users/view-users/view-users.component';

const routes: Routes = [
  {
    path: '',
    redirectTo: 'admin',
    pathMatch: 'full'
  },
  {
    path: 'home', component: HomeComponent
  },

  {
    path: 'asset',
    component: AssetmanagementComponent
  },
  {
    path: 'track',
    component: LivetrackingComponent
  },
  {
    path: 'report',
    component: ReportsComponent
  },
  {
    path: 'admin',
    component: AdminstrationComponent,
    children: [
      {
        path: '',
        redirectTo: 'trailercompany',
        pathMatch: 'full'
      },
      {
        path: 'dashboard',
        component: DashboardComponent
      },
      {
        path: 'trailercompany', component: TrailercompanyComponent,
        children: [
          {
            path: '', redirectTo: 'view',
            pathMatch: 'full'
          },
          {
            path: 'view', component: ViewtrailerdetailComponent
          },
          {
            path: 'Edit', component: CreateAndEditTrailerDataComponent
          }

        ]
      }

      , {
        path: 'dispatchoffice', component: DispatchofficeComponent
        ,
        children: [{
          path: '', redirectTo: 'viewDispatchOffice',
          pathMatch: 'full'
        },

        {
          path: "viewDispatchOffice",
          component: ViewDispatchOfficeComponent
        },
        {
          path: "createUpdateDispatch",
          component: CreateAndUpdateDispatchOfficeComponent
        }

        ]
      }, {
        path: 'locations', component: LocationsComponent

      }
      , {
        path: 'drivers', component: DriversComponent

      }, {
        path: 'users', component: UsersComponent ,
        children: [{
          path: '', redirectTo: 'viewUsers',
          pathMatch: 'full'
        },

        {
          path: "viewUsers",
          component: ViewUsersComponent
        },
        {
          path: "createUpdateUsers",
          component: CreateAndUpdateUsersComponent
        }

        ]

      }, {
        path: 'alerts', component: AlertsComponent

      }, {
        path: 'assets', component: AssetsComponent
      }
    ]
  },

  {
    path: '**', component: PagenotfoundComponent
  }
]


@NgModule({
  declarations: [
    AppComponent,
    HeaderComponent,
    FooterComponent,
    PagenotfoundComponent,
    HomeComponent,
    AssetmanagementComponent,
    LivetrackingComponent,
    ReportsComponent,
    AdminstrationComponent,
    CreateAndEditTrailerDataComponent,
    DashboardComponent,
    TrailercompanyComponent
    , FilterPipe,
    TileComponent,
    ClearinputDirective,
    DispatchofficeComponent,
    LocationsComponent,
    DriversComponent,
    UsersComponent,

    AlertsComponent,
    AssetsComponent,
    ViewtrailerdetailComponent,
    DisableControlDirectiveDirective,
    CreateAndUpdateDispatchOfficeComponent,
    ViewDispatchOfficeComponent,
    ViewUsersComponent,
    CreateAndUpdateUsersComponent
  ],
  imports: [HttpModule, HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    BrowserModule,
    BrowserAnimationsModule,
    OrderModule,
    RouterModule.forRoot(routes),
/*     NgxSpinnerModule,
 */ ToastrModule.forRoot({
      timeOut: 1200,
      positionClass: 'toast-bottom-right',
      preventDuplicates: true,
    }),
    NgbModule.forRoot(),
    TimepickerModule.forRoot(),
    AccordionModule.forRoot(),
    ModalModule.forRoot(),
    PopoverModule.forRoot(),
    PaginationModule.forRoot(),
    ButtonsModule.forRoot(),
    AgmCoreModule.forRoot({
      // please get your own API key here:
      // https://developers.google.com/maps/documentation/javascript/get-api-key?hl=en
      apiKey: 'AIzaSyDpzvxoASC9m4e9AFPMUoLUWQAqcptsn8Q',
      libraries: ["places"]
    })
  ],

//  providers: [AppConfig,,MSALService,AuthenticationHttpInterceptor,ExcelService, CreateTrailerService,  PagerService,CreateDispatchOfficeService,CreateUsersService,
  providers: [AppConfig,,MSALService,AuthenticationHttpInterceptor, CreateTrailerService,  PagerService,CreateDispatchOfficeService,CreateUsersService,
    {
        provide: APP_INITIALIZER,
        useFactory: (config: AppConfig) => () => config.load(),
        deps: [AppConfig],
        multi: true
    },
    {
      provide: APP_INITIALIZER,
      useFactory: (config: AppConfig) => () => config.loadPrivacy(),
      deps: [AppConfig],
      multi: true
    },
    {
      provide: APP_INITIALIZER,
      useFactory: (config: AppConfig) => () => config.loadTerms(),
      deps: [AppConfig],
      multi: true
    },{
      provide: HTTP_INTERCEPTORS,
      useClass: AuthenticationHttpInterceptor,
      multi: true
    }],

  bootstrap: [AppComponent],
  exports: [RouterModule]
})
export class AppModule { }
