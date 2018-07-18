import { Component } from '@angular/core';
import * as $ from 'jquery';
import * as _ from "lodash";
import { MSALService} from './shared/services/msal.service';
import { Router } from '@angular/router';
import { setTheme } from 'ngx-bootstrap/utils';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'app';
  constructor(private router: Router, public service: MSALService){
        setTheme('bs4'); // or 'bs4'
      
  }
  ngOnInit()
  {
    if(this.service.isOnline()) {
      this.router.navigate(['']);
  }else {
    console.log("login");
      this.service.login();
  }
   
    
  }

}
