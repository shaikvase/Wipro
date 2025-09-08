import { Injectable } from '@angular/core';
import { Post } from './post.model';

@Injectable({
  providedIn: 'root',
})
export class DataService {
  posts: Post[] = [];
  allPosts:Post[]=[];
  requestPsot:Post[]=[];
  constructor() {}

  getAllPost() {
    this.posts = localStorage.getItem('posts')
      ? JSON.parse(localStorage.getItem('posts') as string)
      : [];
    return this.posts;
  }
  deletePost(id: any) {
    this.posts = localStorage.getItem('posts')
      ? JSON.parse(localStorage.getItem('posts') as string)
      : [];
    this.posts = this.posts.filter((x: any) => x.id !== id);
    localStorage.setItem('posts', JSON.stringify(this.posts));
  }
  addPost(title: string, content: string, name: string) {
    const newPost = {
      title: title,
      content: content,
      name: name,
      id: new Date(),
    };
    this.posts.push(newPost);
    localStorage.setItem('posts', JSON.stringify(this.posts));
  }
  getSinglePost(id:string){
    this.allPosts=this.getAllPost();
    this.requestPsot=this.allPosts.filter((x:any)=> x.id===id)
    return this.requestPsot;
  }

  editPost(values:any,id:string){
    this.requestPsot[0].title=values.title;
    this.requestPsot[0].content=values.content;
    this.requestPsot[0].name=values.name;
    console.log(this.requestPsot);
    this.posts.filter((x:any)=> {
      if(x.id===id){
        x=this.requestPsot[0];
      }
    });
    localStorage.setItem('posts', JSON.stringify(this.posts));
  }

}
