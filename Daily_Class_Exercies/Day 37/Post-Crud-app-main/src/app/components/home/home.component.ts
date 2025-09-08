import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent {
  constructor(public route:Router){}
  onAddPost(){
    this.route.navigate(['add']);
  }
  connectMe(){
    const link= document.createElement('a');
    link.href='https://www.linkedin.com/in/raghavgarg2002';
    link.click();
  }
}
