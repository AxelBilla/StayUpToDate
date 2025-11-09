import postgres from 'postgres'

const sql = postgres({
    host                 : 'localhost',            // Postgres ip address[s] or domain name[s]
    port                 : 5432,          // Postgres server port[s]
    database             : '',            // Name of database to connect to
    username             : '',            // Username of database user
    password             : '',            // Password of database user
})
// CREATE TABLE reddit(id int primary key, subreddit varchar(21), author varchar(20), title varchar(300), content varchar(40000), flair varchar(64), date timestamp, is_media boolean, url varchar(2048), origin varchar(100));

export default class Database{
    static Add = class{
        static async Post(table, post){
            try {
                await sql`
                    INSERT INTO ${sql(table)}
                    VALUES (${post.id}, ${post.subreddit}, ${post.author}, ${post.title}, ${post.content},
                            ${post.flair}, ${post.date}, ${post.isMedia}, ${post.url}, ${post.origin})
                `
            } catch (e) {
                console.error(`[ERROR] (${post.subreddit} - ${post.id}) This post already exists`);
            }

        }

    }
}
