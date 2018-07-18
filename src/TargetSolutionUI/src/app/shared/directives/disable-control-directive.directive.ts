import { Directive,Input } from '@angular/core';
import { NgControl } from '@angular/forms';

@Directive({
  selector: '[appDisableControlDirective]'
})
export class DisableControlDirectiveDirective {
  @Input() set disableControl( condition : boolean ) {
    const action = 'disable';
    this.ngControl.control[action]();
  }

  constructor( private ngControl : NgControl ) {
  }

}
