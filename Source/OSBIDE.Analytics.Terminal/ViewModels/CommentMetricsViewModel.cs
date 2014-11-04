using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OSBIDE.Analytics.Terminal.ViewModels
{
    public class CommentMetricsViewModel
    {
        private OsbideContext _db { get; set; }
        private List<Comment> _loadedComments { get; set; }

        private Dictionary<string, int> _syllables { get; set; }
        public CommentMetricsViewModel()
        {
            _db = OsbideContext.DefaultWebConnection;
            _loadedComments = new List<Comment>();
            _syllables = new Dictionary<string, int>();
        }

        public void LoadFeedPosts()
        {
            _loadedComments = _loadedComments.Union(GetFeedPosts()).ToList();
        }

        public void LoadLogComments()
        {
            _loadedComments = _loadedComments.Union(GetLogComments()).ToList();
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
            if(_syllables.Count == 0)
            {
                LoadSyllables();
            }

            Regex punctuation = new Regex("[.!?]+");
            string punctuationReplace = ".";
            Regex specialChars = new Regex("[^0-9a-zA-Z. ]+");
            string specialCharsReplace = "";
            char[] wordSeparator = {' '};
            string[] sentenceSeparator = { ". " };

            foreach(Comment comment in _loadedComments)
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
                double averageSentenceLength = (double)comment.WordCount / (double)comment.SentenceCount;

                //determine syllable count
                int validWords = 0;
                int syllableCount = 0;
                foreach(string word in words)
                {
                    //remove periods
                    string modifedWord = word.Replace('.', ' ').ToUpper();
                    if (_syllables.ContainsKey(modifedWord) == true)
                    {
                        validWords++;
                        syllableCount += _syllables[modifedWord];
                    }
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
            var query = from user in _db.Users
                        join log in _db.EventLogs on user.Id equals log.SenderId
                        join post in _db.LogCommentEvents on log.Id equals post.EventLogId
                        select new Comment()
                        {
                            Content = post.Content
                            ,
                            CommentId = post.Id
                            ,
                            CommentType = CommentType.LogComment
                            ,
                            StudentId = user.InstitutionId
                            ,
                            UserId = user.Id
                        };
            List<Comment> comments = query.ToList();
            return comments;
        }

        public List<Comment> GetFeedPosts()
        {
            var query = from user in _db.Users
                        join log in _db.EventLogs on user.Id equals log.SenderId
                        join post in _db.FeedPostEvents on log.Id equals post.EventLogId
                        select new Comment()
                        {
                            Content = post.Comment
                            ,
                            CommentId = post.Id
                            ,
                            CommentType = CommentType.FeedPost
                            ,
                            StudentId = user.InstitutionId
                            ,
                            UserId = user.Id
                        };
            List<Comment> comments = query.ToList();
            return comments;
        }
    }
}
