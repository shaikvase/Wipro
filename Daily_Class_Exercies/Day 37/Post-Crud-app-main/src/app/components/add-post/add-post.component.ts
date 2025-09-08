import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';
import { DataService } from '../data.service';

@Component({
  selector: 'app-add-post',
  templateUrl: './add-post.component.html',
  styleUrls: ['./add-post.component.scss'],
})
export class AddPostComponent implements OnInit {
  myForm!: FormGroup;
  constructor(
    private route: Router,
    public data: DataService,
    private _snackBar: MatSnackBar,
    public actiRoute: ActivatedRoute
  ) {}
  ngOnInit(): void {
    this.myForm = new FormGroup({
      title: new FormControl('', Validators.required),
      content: new FormControl('', Validators.required),
      name: new FormControl('', Validators.required),
    });
  }

  onAddingPost() {
    this.data.addPost(
      this.myForm.value.title,
      this.myForm.value.content,
      this.myForm.value.name
    );
    this.openSnackBar('Post Added', 'X');
    this.route.navigate(['']);
  }

  openSnackBar(message: string, action: string) {
    this._snackBar.open(message, action, { duration: 2000 });
  }
}
