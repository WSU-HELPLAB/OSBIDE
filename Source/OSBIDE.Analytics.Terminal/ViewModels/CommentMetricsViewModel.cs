using OSBIDE.Library.CSV;
using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OSBIDE.Analytics.Terminal.ViewModels
{
    public class CommentMetricsViewModel
    {
        private OsbideContext _db { get; set; }
        private List<Comment> _loadedComments { get; set; }
        private Dictionary<int, Comment> _threadedComments { get; set; }
        private Dictionary<string, int> _syllables { get; set; }
        public CommentMetricsViewModel()
        {
            _db = OsbideContext.DefaultWebConnection;
            _loadedComments = new List<Comment>();
            _syllables = new Dictionary<string, int>();
            _threadedComments = new Dictionary<int, Comment>();
        }

        public void OrganizeComments()
        {
            //holds temporarily orphaned comments
            Dictionary<int, Comment> pendingComments = new Dictionary<int, Comment>();

            //first pass: load in comments
            foreach(Comment comment in _loadedComments)
            {
                //top level post or comment?
                if(comment.CommentType == CommentType.FeedPost)
                {
                    //do we have record of this comment?
                    if (_threadedComments.ContainsKey(comment.CommentId) == false)
                    {
                        _threadedComments.Add(comment.CommentId, comment);
                    }
                }
                else
                {
                    //do we have record of this comment's parent?
                    if(_threadedComments.ContainsKey(comment.ParentId) == true)
                    {
                        //if so, add to list of child comments
                        _threadedComments[comment.ParentId].ChildComments.Add(comment);
                    }
                    else
                    {
                        //otherwise, we need to add to orphaned list
                        pendingComments[comment.CommentId] = comment;
                    }
                }
            }

            //2nd pass: add in orphaned comments
            foreach(Comment comment in pendingComments.Values)
            {
                //by now, this should always be true
                if (_threadedComments.ContainsKey(comment.ParentId) == true)
                {
                    //add to list of child comments
                    _threadedComments[comment.ParentId].ChildComments.Add(comment);
                }
            }

            //3rd pass: sort comments
            foreach(KeyValuePair<int, Comment> kvp in _threadedComments)
            {
                kvp.Value.ChildComments = kvp.Value.ChildComments.OrderBy(c => c.DateReceived).ToList();
            }
        }

        public void LoadFeedPosts()
        {
            foreach(Comment comment in GetFeedPosts())
            {
                _loadedComments.Add(comment);
            }
        }

        public void LoadLogComments()
        {
            foreach (Comment comment in GetLogComments())
            {
                _loadedComments.Add(comment);
            }
        }

        public void LoadSyllables()
        {
            _syllables = _db.Syllables.ToDictionary(m => m.Word, m => m.SyllableCount);
        }

        /// <summary>
        /// Calculates word metrics for all loaded comments
        /// </summary>
        public void AnalyzeComments()
        {
            //check to make sure that we have syllables loaded
            if (_syllables.Count == 0)
            {
                LoadSyllables();
            }

            Regex punctuation = new Regex("[.!?]+");
            string punctuationReplace = ".";
            Regex specialChars = new Regex("[^0-9a-zA-Z. ]+");
            string specialCharsReplace = "";
            char[] wordSeparator = { ' ' };
            string[] sentenceSeparator = { ". " };

            foreach (Comment comment in _loadedComments)
            {
                //sanitize data
                string content = punctuation.Replace(comment.Content, punctuationReplace);
                content = specialChars.Replace(content, specialCharsReplace);
                content = content.Trim();

                //split into words
                string[] words = content.Split(wordSeparator, StringSplitOptions.RemoveEmptyEntries);

                //and sentences
                string[] sentences = content.Split(sentenceSeparator, StringSplitOptions.RemoveEmptyEntries);

                //save word count
                comment.WordCount = words.Length;
                comment.SentenceCount = sentences.Length;

                //prevent divide by zero errors
                if(comment.SentenceCount == 0)
                {
                    comment.SentenceCount = 1;
                }
                double averageSentenceLength = (double)comment.WordCount / (double)comment.SentenceCount;

                //determine syllable count
                int validWords = 0;
                int syllableCount = 0;
                foreach (string word in words)
                {
                    //remove punctuation
                    string modifedWord = word.Replace('.', ' ').ToUpper();

                    if (_syllables.ContainsKey(modifedWord) == true)
                    {
                        validWords++;
                        syllableCount += _syllables[modifedWord];
                    }
                }

                //prevent divide by zero errors
                if(syllableCount == 0)
                {
                    syllableCount = 1;
                }
                comment.AverageSyllablesPerWord = (double)validWords / (double)syllableCount;

                //determine Flesch Reading Ease
                comment.FleschReadingEase = 206.835 - (1.015 * averageSentenceLength) - (84.6 * comment.AverageSyllablesPerWord);

                //determine reading level
                comment.FleschKincaidGradeLevel = (0.39 * averageSentenceLength) + (11.8 * comment.AverageSyllablesPerWord) - 15.59;
            }
        }

        public List<Comment> GetLogComments()
        {
            //TODO: allow custom timespan inputs
            DateTime startDate = new DateTime(2014, 01, 01);
            DateTime endDate = new DateTime(2014, 05, 15);

            var query = from user in _db.Users
                        join log in _db.EventLogs on user.Id equals log.SenderId
                        join post in _db.LogCommentEvents on log.Id equals post.EventLogId
                        where log.DateReceived >= startDate
                              && log.DateReceived <= endDate
                        select new Comment()
                        {
                            Content = post.Content
                            ,
                            CommentId = log.Id
                            ,
                            DateReceived = log.DateReceived
                            ,
                            ParentId = post.SourceEventLogId
                            ,
                            CommentType = CommentType.LogComment
                            ,
                            StudentId = user.InstitutionId
                            ,
                            UserId = user.Id
                            ,
                            User = user
                        };
            List<Comment> comments = query.ToList();
            return comments;
        }

        public List<Comment> GetFeedPosts()
        {
            //TODO: allow custom timespan inputs
            DateTime startDate = new DateTime(2014, 01, 01);
            DateTime endDate = new DateTime(2014, 05, 15);

            var query = from user in _db.Users
                        join log in _db.EventLogs on user.Id equals log.SenderId
                        join post in _db.FeedPostEvents on log.Id equals post.EventLogId
                        where
                                 log.DateReceived >= startDate
                              && log.DateReceived <= endDate
                              && (   
                                user.CourseUserRelationships.Where(c => c.CourseId == 3).Count() > 0
                                    )
                        select new Comment()
                        {
                            Content = post.Comment
                            ,
                            CommentId = log.Id
                            ,
                            DateReceived = log.DateReceived
                            ,
                            ParentId = -1
                            ,
                            CommentType = CommentType.FeedPost
                            ,
                            StudentId = user.InstitutionId
                            ,
                            UserId = user.Id
                            ,
                            User = user
                        };
            List<Comment> comments = query.ToList();
            return comments;
        }

        public void WriteToCsv(string fileName)
        {
            List<int> keys = _threadedComments.Keys.ToList();
            keys.Sort();

            CsvWriter csvWriter = new CsvWriter();

            //write header
            csvWriter.AddToCurrentLine("Post ID");
            csvWriter.AddToCurrentLine("Date");
            csvWriter.AddToCurrentLine("Author");
            csvWriter.AddToCurrentLine("AuthorRole");
            csvWriter.AddToCurrentLine("Content");
            csvWriter.AddToCurrentLine("Reading Ease");
            csvWriter.AddToCurrentLine("Reading Grade Level");
            csvWriter.CreateNewRow();

            //write entries
            int commentCounter = 1;
            foreach(int key in keys)
            {
                Comment rootComment = _threadedComments[key];

                //write root, then all child comments
                csvWriter.AddToCurrentLine(commentCounter.ToString());
                csvWriter.AddToCurrentLine(rootComment.DateReceived.ToString("yyyy-MM-dd HH:mm:ss"));
                csvWriter.AddToCurrentLine(rootComment.UserId.ToString());
                csvWriter.AddToCurrentLine(rootComment.User.Role.ToString());
                csvWriter.AddToCurrentLine(rootComment.Content);
                csvWriter.AddToCurrentLine(rootComment.FleschReadingEase.ToString());
                csvWriter.AddToCurrentLine(rootComment.FleschKincaidGradeLevel.ToString());
                csvWriter.CreateNewRow();

                //now do all children
                int childCounter = 1;
                foreach(Comment child in rootComment.ChildComments)
                {
                    csvWriter.AddToCurrentLine(string.Format("{0}.{1}", commentCounter, childCounter));
                    csvWriter.AddToCurrentLine(child.DateReceived.ToString("yyyy-MM-dd HH:mm:ss"));
                    csvWriter.AddToCurrentLine(child.UserId.ToString());
                    csvWriter.AddToCurrentLine(child.User.Role.ToString());
                    csvWriter.AddToCurrentLine(child.Content);
                    csvWriter.AddToCurrentLine(child.FleschReadingEase.ToString());
                    csvWriter.AddToCurrentLine(child.FleschKincaidGradeLevel.ToString());
                    csvWriter.CreateNewRow();
                    childCounter++;
                }
                commentCounter++;
            }
            using (TextWriter tw = File.CreateText(fileName))
            {
                tw.Write(csvWriter.ToString());
            }
        }
    }
}
