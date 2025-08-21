DROP TABLE IF EXISTS PostTags;
DROP TABLE IF EXISTS PostReactions;
DROP TABLE IF EXISTS Comments;
DROP TABLE IF EXISTS Posts;
DROP TABLE IF EXISTS Subscriptions;
DROP TABLE IF EXISTS Users;
DROP TABLE IF EXISTS Reactions;
DROP TABLE IF EXISTS Tags;
DROP TABLE IF EXISTS Categories;

CREATE TABLE "Users" (
  id SERIAL PRIMARY KEY,
  first_name VARCHAR,
  last_name VARCHAR,
  email VARCHAR,
  bio VARCHAR,
  username VARCHAR,
  password VARCHAR,
  profile_image_url VARCHAR,
  created_on DATE,
  active BOOLEAN
);

CREATE TABLE "Categories" (
  id SERIAL PRIMARY KEY,
  label VARCHAR
);

CREATE TABLE "Posts" (
  id SERIAL PRIMARY KEY,
  user_id INTEGER,
  category_id INTEGER,
  title VARCHAR,
  publication_date DATE,
  image_url VARCHAR,
  content VARCHAR,
  approved BOOLEAN,
  FOREIGN KEY(user_id) REFERENCES "Users"(id),
  FOREIGN KEY(category_id) REFERENCES "Categories"(id)
);

CREATE TABLE "Subscriptions" (
  id SERIAL PRIMARY KEY,
  follower_id INTEGER,
  author_id INTEGER,
  created_on DATE,
  FOREIGN KEY(follower_id) REFERENCES "Users"(id),
  FOREIGN KEY(author_id) REFERENCES "Users"(id)
);

CREATE TABLE "Comments" (
  id SERIAL PRIMARY KEY,
  post_id INTEGER,
  author_id INTEGER,
  content VARCHAR,
  created_on DATE,
  FOREIGN KEY(post_id) REFERENCES "Posts"(id),
  FOREIGN KEY(author_id) REFERENCES "Users"(id)
);

CREATE TABLE "Reactions" (
  id SERIAL PRIMARY KEY,
  label VARCHAR,
  image_url VARCHAR
);

CREATE TABLE "PostReactions" (
  id SERIAL PRIMARY KEY,
  user_id INTEGER,
  reaction_id INTEGER,
  post_id INTEGER,
  FOREIGN KEY(user_id) REFERENCES "Users"(id),
  FOREIGN KEY(reaction_id) REFERENCES "Reactions"(id),
  FOREIGN KEY(post_id) REFERENCES "Posts"(id)
);

CREATE TABLE "Tags" (
  id SERIAL PRIMARY KEY,
  label VARCHAR
);

CREATE TABLE "PostTags" (
  id SERIAL PRIMARY KEY,
  post_id INTEGER,
  tag_id INTEGER,
  FOREIGN KEY(post_id) REFERENCES "Posts"(id),
  FOREIGN KEY(tag_id) REFERENCES "Tags"(id)
);