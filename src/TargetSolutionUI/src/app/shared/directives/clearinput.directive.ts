import { Directive,ElementRef,Input, HostListener} from '@angular/core';

@Directive({
  selector: '[appClearinput]'
})
export class ClearinputDirective {
  @Input() appClearinput(event): void {
console.log(event.nativeElement.value)
  }
  constructor(private el: ElementRef) {
    console.log(el.nativeElement.value);
  }

  @HostListener('document:click', ['$event'])
  handleClick(event: Event) {
    if (this.el.nativeElement.contains(event.target)) {
      console.log(this.el.nativeElement.contains(event.target));
    } else {


    }
  }

}
