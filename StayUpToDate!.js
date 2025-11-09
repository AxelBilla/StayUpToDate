import { createRequire } from "module";
import { fileURLToPath } from "url";
import { dirname } from "path";
import schedule from 'node-schedule'

import { default as db } from "./db.connect.js";

const require = createRequire(import.meta.url);
const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);
const home_path = require("path");

const { exec } = require('child_process');
const fs = require('fs');

class App{
    static path = __dirname+"/Scrapper/files/";
    static Start(delay_cron){App.Update()
        schedule.scheduleJob(delay_cron, () => { App.Update(); })
    }
    
    static async Update(){
        
        App.Scrape();

        setTimeout(async function(){
            App.getData();
        }, 2000);
    }
    
    static async getData(){
        let day = new Date();
        let today = ("0" + day.getDate()).slice(-2) + "-" + ("0"+(day.getMonth()+1)).slice(-2) + "-" +
            day.getFullYear();

        let subs = fs.readdirSync(App.path+today);
        for(let sub of subs){
            let entries = fs.readdirSync(App.path+today+"/"+sub);
            for(let entry of entries) {
                let fl_path = `${App.path}${today}/${sub}/${entry}`;
                let posts = JSON.parse(App.Read.File(fl_path)).data.children;
                for (let post of posts) {
                    let reformatted_post = Reddit.Format.Post(post.data);
                    await db.Add.Post("reddit", reformatted_post);
                }
            }
        }
    }
    static Read = class{
        static File(path){
            return fs.readFileSync(path, 'utf-8', (err)=>{console.log(err)});
        }
    }
    
    static credentials = JSON.parse(App.Read.File("./credentials.json"));
    static scrape_cmd = `./RedditScrapper -id=${this.credentials["id"]} -secret=${this.credentials["secret"]}`;
    static async Scrape(){
        await exec(this.scrape_cmd, {cwd: './Scrapper'});
    }
}


class Reddit{
    static Format = class{
        static Post(post){
            return new Reddit.Post(post.id, post.subreddit, post.author, post.title, post.selftext, post.link_flair_text, post.created_utc, post.is_reddit_media_domain, post.url ,post.permalink);
        }
    }
    static Post = class{
        constructor(id, subreddit, author, title="n/a", content="n/a", flair="none", date, isMedia, url, origin){
            this.id = id;
            this.subreddit = subreddit;
            this.author = author;
            this.title = title;
            this.content = content;
            this.flair = flair;
            this.date = new Date(date*1000);
            this.isMedia = isMedia;
            this.url = url;
            this.origin = origin;
        }
    }
}

App.Start("0 * * * *");
