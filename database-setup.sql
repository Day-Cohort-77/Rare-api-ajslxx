-- DELETE FROM "PostTags";
-- DELETE FROM "PostReactions";
-- DELETE FROM "Comments";
-- DELETE FROM "Posts";
-- DELETE FROM "Subscriptions";
-- DELETE FROM "Users";
-- DELETE FROM "Reactions";
-- DELETE FROM "Tags";
-- DELETE FROM "Categories";

-- Drop tables in correct order to handle foreign key dependencies
DROP TABLE IF EXISTS "PostTags";
DROP TABLE IF EXISTS "PostReactions";
DROP TABLE IF EXISTS "Comments";
DROP TABLE IF EXISTS "Posts";
DROP TABLE IF EXISTS "Subscriptions";
DROP TABLE IF EXISTS "Users";
DROP TABLE IF EXISTS "Reactions";
DROP TABLE IF EXISTS "Tags";
DROP TABLE IF EXISTS "Categories";

CREATE TABLE IF NOT EXISTS "Users" (
  id SERIAL PRIMARY KEY,
  first_name VARCHAR,
  last_name VARCHAR,
  email VARCHAR,
  bio VARCHAR,
  username VARCHAR,
  password VARCHAR,
  profile_image_url VARCHAR,
  created_on TIMESTAMP,
  active BOOLEAN
);

CREATE TABLE IF NOT EXISTS "Categories" (
  id SERIAL PRIMARY KEY,
  label VARCHAR
);

CREATE TABLE IF NOT EXISTS "Subscriptions" (
  id SERIAL PRIMARY KEY,
  follower_id INTEGER,
  author_id INTEGER,
  created_on TIMESTAMP,
  FOREIGN KEY(follower_id) REFERENCES "Users"(id),
  FOREIGN KEY(author_id) REFERENCES "Users"(id)
);

CREATE TABLE IF NOT EXISTS "Posts" (
  id SERIAL PRIMARY KEY,
  user_id INTEGER,
  category_id INTEGER,
  title VARCHAR,
  publication_date TIMESTAMP,
  image_url VARCHAR,
  content VARCHAR,
  approved BOOLEAN,
  FOREIGN KEY(user_id) REFERENCES "Users"(id),
  FOREIGN KEY(category_id) REFERENCES "Categories"(id)
);

CREATE TABLE IF NOT EXISTS "Comments" (
  id SERIAL PRIMARY KEY,
  post_id INTEGER,
  author_id INTEGER,
  content VARCHAR,
  subject VARCHAR(255),
  created_on TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY(post_id) REFERENCES "Posts"(id),
  FOREIGN KEY(author_id) REFERENCES "Users"(id)
);

CREATE TABLE IF NOT EXISTS "Reactions" (
  id SERIAL PRIMARY KEY,
  label VARCHAR,
  image_url VARCHAR
);

CREATE TABLE IF NOT EXISTS "PostReactions" (
  id SERIAL PRIMARY KEY,
  user_id INTEGER,
  reaction_id INTEGER,
  post_id INTEGER,
  FOREIGN KEY(user_id) REFERENCES "Users"(id),
  FOREIGN KEY(reaction_id) REFERENCES "Reactions"(id),
  FOREIGN KEY(post_id) REFERENCES "Posts"(id)
);

CREATE TABLE IF NOT EXISTS "Tags" (
  id SERIAL PRIMARY KEY,
  label VARCHAR
);

CREATE TABLE IF NOT EXISTS "PostTags" (
  id SERIAL PRIMARY KEY,
  post_id INTEGER,
  tag_id INTEGER,
  FOREIGN KEY(post_id) REFERENCES "Posts"(id),
  FOREIGN KEY(tag_id) REFERENCES "Tags"(id)
);

-- Add missing columns to Comments table
ALTER TABLE "Comments" ADD COLUMN IF NOT EXISTS subject VARCHAR(255);
ALTER TABLE "Comments" ADD COLUMN IF NOT EXISTS created_on TIMESTAMP DEFAULT CURRENT_TIMESTAMP;

-- Create indexes for performance
CREATE INDEX IF NOT EXISTS idx_comments_post_created ON "Comments"(post_id, created_on DESC);
CREATE INDEX IF NOT EXISTS idx_comments_author ON "Comments"(author_id);

-- For efficient comment retrieval by post (most important)
CREATE INDEX idx_comments_post_created ON "Comments"(post_id, created_on DESC);

-- For efficient JOIN operations
CREATE INDEX idx_comments_author ON "Comments"(author_id);
CREATE INDEX idx_users_id ON "Users"(id);  -- If not exists
CREATE INDEX idx_posts_id ON "Posts"(id);  -- If not exists