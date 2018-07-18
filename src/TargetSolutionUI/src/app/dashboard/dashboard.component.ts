import { Component, Injector, OnInit, Input } from '@angular/core';
import { FormGroup, FormBuilder, Validators, FormArray } from '@angular/forms';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {


    constructor(injector: Injector,
                private fb: FormBuilder,
      ) { 
          
      }
      
      ngOnInit(): void {
      
     
      }
      
   
      
     
      
  

}
