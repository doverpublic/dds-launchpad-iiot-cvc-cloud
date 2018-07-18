import { Component, OnInit ,ViewChild} from '@angular/core';
import { Router } from '@angular/router';
import { MSALService} from '../shared/services/msal.service';

@Component({
  selector: 'app-adminstration',
  templateUrl: './adminstration.component.html',
  styleUrls: ['./adminstration.component.scss']
})
export class AdminstrationComponent implements OnInit {
  ngOnInit()
  {
   
    
  }
  constructor(private router: Router, public service: MSALService){    
  }
}
