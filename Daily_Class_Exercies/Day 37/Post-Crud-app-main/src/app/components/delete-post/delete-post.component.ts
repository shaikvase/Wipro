import { Component, Input, OnInit, Inject } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { DataService } from '../data.service';
import { Post } from '../post.model';

@Component({
  selector: 'app-delete-post',
  templateUrl: './delete-post.component.html',
  styleUrls: ['./delete-post.component.scss'],
})
export class DeletePostComponent implements OnInit {
  myForm!: FormGroup;
  requestPost:Post[]=[];
  constructor(@Inject(MAT_DIALOG_DATA) public data: { option: string,id:string },
  public dataServ:DataService) {}
  ngOnInit(): void {
    if (this.data.option === 'edit') {
      this.myForm = new FormGroup({
        title: new FormControl('', Validators.required),
        content: new FormControl('', Validators.required),
        name: new FormControl('', Validators.required),
      });
      this.prePopulate();
    }
  }

  prePopulate(){
    this.requestPost=this.dataServ.getSinglePost(this.data.id);
    this.myForm.patchValue({
      title:this.requestPost[0].title,
      content:this.requestPost[0].content,
      name:this.requestPost[0].name,
    });
  }

  onEditPost(){
    this.dataServ.editPost(this.myForm.value,this.data.id);
  }
}
