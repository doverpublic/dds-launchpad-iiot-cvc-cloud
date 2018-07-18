import { Component, OnInit } from '@angular/core';
import { MSALService} from '../shared/services/msal.service';
import { Router } from '@angular/router';
@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {

  constructor(private router: Router, public service: MSALService){
  }
  ngOnInit() {
    
   /*  if(this.service.isOnline()) {
      this.router.navigate(['']);
  }else {
    console.log("login");
      this.service.login();
  } */
   
  }

}
