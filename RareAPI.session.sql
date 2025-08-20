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

CREATE TABLE "Subscriptions" (
  id SERIAL PRIMARY KEY,
  follower_id INTEGER,
  author_id INTEGER,
  created_on DATE,
  FOREIGN KEY(follower_id) REFERENCES "Users"(id),
  FOREIGN KEY(author_id) REFERENCES "Users"(id)
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
  FOREIGN KEY(user_id) REFERENCES "Users"(id)
);

CREATE TABLE "Comments" (
  id SERIAL PRIMARY KEY,
  post_id INTEGER,
  author_id INTEGER,
  content VARCHAR,
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

CREATE TABLE "Categories" (
  id SERIAL PRIMARY KEY,
  label VARCHAR
);

INSERT INTO "Categories" (label) VALUES ('News');
INSERT INTO "Tags" (label) VALUES ('JavaScript');
INSERT INTO "Reactions" (label, image_url) VALUES ('happy', 'https://pngtree.com/so/happy');