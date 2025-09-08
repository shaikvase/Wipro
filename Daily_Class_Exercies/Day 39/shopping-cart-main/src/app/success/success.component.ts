import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ProductService } from '../product.service';
@Component({
  selector: 'app-success',
  templateUrl: './success.component.html',
  styleUrls: ['./success.component.scss']
})
export class SuccessComponent implements OnInit {

  constructor(
    private product_service: ProductService,
    private router: Router
  ) { }
  ngOnInit(): void { }

  goBackToHome() {
    this.product_service.clearProducts();
    this.router.navigate(['/']);
  }
}
