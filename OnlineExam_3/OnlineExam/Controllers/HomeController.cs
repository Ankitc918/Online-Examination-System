using OnlineExam.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections;
using System.Net.Mail;
using System.Net;

namespace OnlineExamSystem.Controllers
{
    public class HomeController : Controller
    {
        OnlineExamEntities4 db = new OnlineExamEntities4();
        public ActionResult Index()
        {
                var _ctx = new OnlineExamEntities4();
                ViewBag.Tests = _ctx.Tests.Where(x => x.IsActive == true).Select(x => new { x.Id, x.Name }).ToList();
                SessionModel _model = null;
                if (Session["SessionModel"] == null)
                    _model = new SessionModel();
                else
                    _model = (SessionModel)Session["SessionModel"];

                return View(_model);
         }
        [HttpPost]
        public ActionResult Index(SessionModel user)
        {
            if (user != null)
            {
               
                var u = db.Users.ToList();
                foreach(var n in u)
                {
                    if (n.Name == user.UserName && n.Email == user.Email
                        && n.IsValid==true
                        )
                    {
                        return RedirectToAction("Instruction", user);
                    }
                   
                }
                TempData["message"] = "You are not registered! Please register first.";
              
                return RedirectToAction("Index");
            }
            return View("~/Views/Shared/Error.cshtml");
        }

        // only used for activating from mail link . this action is called when you hit the link in email.
        public ActionResult Activation()
        {
            ViewBag.Message = "Invalid Activation code.";
            if (RouteData.Values["id"] != null)
            {
                Guid activationCode = new Guid(RouteData.Values["id"].ToString());
                var userActivation = db.Users.Where(p => p.ActivationCode == activationCode).FirstOrDefault();
                if (userActivation != null)
                {
                    userActivation.IsValid = true;
                    db.SaveChanges();
                    TempData["message"] = "Your registration was successful.";
                    return RedirectToAction("Index");
                }
            }

           return View("~/Views/Shared/Error.cshtml");
        }
 
        // for sending mail for activation
        private void sendActivationMail(SessionModel user)
        {
            Guid activationCode = Guid.NewGuid();

            
                using (MailMessage mm = new MailMessage("onlineexamsystembyankit@gmail.com", user.Email))
                {
                    mm.Sender = new MailAddress("onlineexamsystembyankit@gmail.com");
                    mm.Subject = "Registration";
                    string body = "Hello <strong>" + user.UserName + "</strong>,<br /><br />Please click the following link to register for examination <br /><a href = '" + string.Format("{0}://{1}//Home/Activation/{2}", Request.Url.Scheme, Request.Url.Authority, activationCode) + "'>Click here to register.</a> <br /><br />Thanks, <br/>Regards,<br/> Ankit Chauhan";
                    // if publishing to localhost then set string path in body as : "{0}://{1}/'localhost site name here'/Home/Activation/{2}"
                    mm.Body = body;
                    mm.IsBodyHtml = true;
                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = "smtp.gmail.com";
                    smtp.EnableSsl = true;
                    NetworkCredential NetworkCred = new NetworkCredential("onlineexamsystembyankit@gmail.com", "OnlineExamSystem123");
                    smtp.UseDefaultCredentials = false;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Credentials = NetworkCred;
                    //smtp.Port = 587;
                    smtp.Send(mm);

                    var exist = db.Users.Where(x => x.Name == user.UserName && x.Email == user.Email).FirstOrDefault();
                    if (exist == null)
                    {
                        db.Users.Add(new User                                   // add user in database after mail is sent successfully
                        {
                            Name = user.UserName,
                            AccessLevel = "user",
                            EntryDate = DateTime.Now.Date,
                            Email = user.Email,
                            ActivationCode = activationCode
                        });
                    }
                    else
                    {
                        exist.ActivationCode = activationCode;
                    }

                    db.SaveChanges();

                }
            
          
            
        }

        public ActionResult RegNew() 
        {
            return View();
        }
        [HttpPost]
        public ActionResult RegNew(SessionModel user)
        {
            if (user != null)
            {
                sendActivationMail(user);
                TempData["message"] = " Activation link has been sent to your E-mail .<br/>Click on the link sent to your E-mail to register yourself and then start the test.<br/>Thanks.";
                return View("~/Views/Home/Activation.cshtml");
            }
            TempData["message"] = "Invalid Details. Please retry.";
            return View();
        }

        public ActionResult Instruction(SessionModel model) 
        {
            if (model != null)
            {
                var _ctx = new OnlineExamEntities4();
                var test = _ctx.Tests.Where(x => x.IsActive == true && x.Id == model.TestId).FirstOrDefault();
                if (test != null)
                {
                    ViewBag.TestName = test.Name;
                    ViewBag.TestDescription = test.Description;
                    ViewBag.QuestionCount = test.TestXQuestions.Count;
                    ViewBag.TestDuration = test.DurationInMinute;
                }
            }
            return View(model);
        }
        public ActionResult Registration(SessionModel model) 
        {
            if (model != null)
                Session["SessionModel"] = model;
            if (model == null || string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.Email)
               // || string.IsNullOrEmpty(model.Password) 
                || model.TestId < 1)
            {
                TempData["message"] = "Invalid Registration Details. Please try again.";
                return RedirectToAction("Index");
            }
            var _ctx = new OnlineExamEntities4();
            //to register the user to system
            //to register the user for test
            User _user = _ctx.Users.Where(x => x.Name.Equals(model.UserName, StringComparison.InvariantCultureIgnoreCase)
                && ((string.IsNullOrEmpty(model.Email) && string.IsNullOrEmpty(x.Email)) || (x.Email == model.Email))
                //&& ((string.IsNullOrEmpty(model.Password) && string.IsNullOrEmpty(x.Password)) || (x.Password == model.Password))
                ).FirstOrDefault();

            if (_user == null)
            {
                _user = new User()
                {
                    Name = model.UserName,
                    Email = model.Email,
                    //Password = model.Password,
                    EntryDate = DateTime.Now,
                    AccessLevel ="user",
                    IsValid=true
                };
                _ctx.Users.Add(_user);
                _ctx.SaveChanges();
            }
            var time = DateTime.Now.TimeOfDay;
            Registration registration = _ctx.Registrations.Where(x => x.UserId == _user.Id
                && x.TestId == model.TestId
                && x.TokenExpireTime >time
                ).FirstOrDefault();
            if (registration != null)
            {
                this.Session["TOKEN"] = registration.Token;
                //this.Session["TOKENEXPIRE"] = registration.TokenExpireTime;
            }
            else
            {
                Test test = _ctx.Tests.Where(x => x.IsActive==true && x.Id == model.TestId).FirstOrDefault();
                if (test != null)
                {
                    Registration newRegistration = new Registration()
                    {
                        RegistrationDate = DateTime.Now,
                        TestId = model.TestId,
                        Token = Guid.NewGuid(),
                        TokenExpireTime =time+test.DurationInMinute  // add time test.DurationInMinute to current time
                    };
                    _user.Registrations.Add(newRegistration);
                    _ctx.Registrations.Add(newRegistration);
                    _ctx.SaveChanges();
                    this.Session["TOKEN"] = newRegistration.Token;
                   // this.Session["TOKENEXPIRE"] = newRegistration.TokenExpireTime;
                }
            }


            return RedirectToAction("EvalPage", new { @token=Session["TOKEN"]});
        }

        public ActionResult EvalPage(Guid token,int? qno)
        {
            if (token == null)
            {
                TempData["message"] = "you have an invalid token. Please re-register and try again";
                return RedirectToAction("Index");
            }
            var _ctx = new OnlineExamEntities4();
            // verify the user is registered and is allowed to check the question
            var registration = _ctx.Registrations.Where(x => x.Token==(token)).FirstOrDefault();
            if (registration == null)
            {
                TempData["message"] = "this registration is invalid";
                return RedirectToAction("Index");
            }
            if (registration.TokenExpireTime < DateTime.Now.TimeOfDay)
            {
                TempData["message"] = "the exam duration has expired at "+registration.TokenExpireTime.ToString();
                //_ctx.Registrations.Remove(registration);
                //_ctx.SaveChanges();
                return RedirectToAction("Result", new { @token=Session["TOKEN"]});
            }
            if (qno.GetValueOrDefault() < 1)
                qno = 1;
            var testQuestionId = _ctx.TestXQuestions
                .Where(x => x.TestId == registration.TestId && x.QuestionNumber==qno)
                .Select(x=>x.Id).FirstOrDefault();


            if (testQuestionId > 0)
            {
                var _model = _ctx.TestXQuestions.ToList().Where(x => x.Id == testQuestionId)
                      .Select(x => new QuestionModel()
                      {
                          QuestionType = x.Question.QuestionType,
                          QuestionNumber = x.QuestionNumber,
                          Question = x.Question.Question1,
                          Point = x.Question.Points,
                          TestId = x.TestId,
                          TestName = x.Test.Name,
                          Options = x.Question.Choices.Where(y => y.IsActive == true).Select(y => new QXOModel()
                          {
                              ChoiceId = y.Id,
                              Label = y.Label
                          }).ToList(),

                      }).FirstOrDefault();

                // now if it is already answered earlier, set the choice of the user
              
                var savedAnswers = _ctx.TestXPapers.Where(x => x.TestXQuestionId == testQuestionId && x.RegistrationId == registration.Id && x.Choice.IsActive == true)
                    .Select(x => new { x.ChoiceId, x.Answer }).ToList();

                foreach (var savedAnwer in savedAnswers)
                {
                    _model.Options.Where(x => x.ChoiceId == savedAnwer.ChoiceId).FirstOrDefault().Answer = savedAnwer.Answer;
                }

               _model.TotalQuestionInSet=_ctx.TestXQuestions.Where(x=>x.Question.IsActive==true && x.TestId==registration.TestId).Count();
               ViewBag.TimeExpire = (registration.TokenExpireTime);
                return View(_model);
            }
            else
            {
                return View("Error");
            
            }
        }

        [HttpPost]
        public ActionResult PostAnswer(AnswerModel choices)
        {
           
            var _ctx = new OnlineExamEntities4();
            // verify the user is registered and is allowed to check the question
            var registration = _ctx.Registrations.Where(x => x.Token==(choices.Token)).FirstOrDefault();
            if (registration == null)
            {
                TempData["message"] = "this token is invalid";
                return RedirectToAction("Index");
            }
            if (registration.TokenExpireTime < DateTime.Now.TimeOfDay)
            {
                TempData["message"] = "the exam duration has expired at" + registration.TokenExpireTime.ToString();
                //_ctx.Registrations.Remove(registration);
                //_ctx.SaveChanges();
                return RedirectToAction("Index");
            }

            // to check the answered question is answered or not . If answered then store them in database and evaluate the marks scored.
            
            var testQuestionInfo = _ctx.TestXQuestions.Where(x => x.TestId == registration.TestId
                && x.QuestionNumber == choices.QuestionId)
                .Select(x => new { TQId = x.Id,
                    QT = x.Question.QuestionType,
                    QID = x.Id,
                    POINT = (decimal)x.Question.Points 
                }).FirstOrDefault();
           
            if (testQuestionInfo != null)
            {
                
                if (choices.UserChoices.Count() > 0) // if choices is of radio or checkbox type
                {
                    var c=choices.UserSelectedId.ToList();
                    var quesid = (c.Count()!=0)? _ctx.Choices.Where(x => x.QuestionId == testQuestionInfo.TQId).Select(x=>x.QuestionId ).FirstOrDefault() : 0;
                    var testxpaperlist=_ctx.TestXPapers.Where(x=>x.RegistrationId==registration.Id && x.TestXQuestionId==quesid ).FirstOrDefault();
                    var allPointValueOfChoices =
                        (
                        from a in _ctx.Choices.Where(x => x.IsActive).AsEnumerable()
                        join b in c on a.Id equals b.ChoiceId
                        select new { a.Id, Point = (decimal)a.Points }).AsEnumerable()
                        .Select(x => new TestXPaper()
                        {
                            RegistrationId = registration.Id,
                            TestXQuestionId = testQuestionInfo.QID,
                            ChoiceId = x.Id,
                            Answer = "checked",
                            MarkScored = Math.Floor((decimal) _ctx.Choices.Where(y=>y.Id==x.Id && y.Points>0).Select(p=>p.Question.Points).FirstOrDefault())  // getting the points of the question for correct answer
                        }).ToList();


                    if (testxpaperlist != null )//&& testxpaperlist.RegistrationId == registration.Id && testxpaperlist.TestXQuestionId == testQuestionInfo.QID)
                    {
                        testxpaperlist.ChoiceId = allPointValueOfChoices[0].ChoiceId;
                        testxpaperlist.Answer = allPointValueOfChoices[0].Answer;
                        testxpaperlist.MarkScored = allPointValueOfChoices[0].MarkScored;
                    }
                    else
                    {
                        if(c.Count!=0)
                          _ctx.TestXPapers.Add(allPointValueOfChoices[0]);
                    }
                    _ctx.SaveChanges();

                }
                else
                {
                    //answer of the type text
                    _ctx.TestXPapers.Add(new TestXPaper()
                    {
                        RegistrationId = registration.Id,
                        TestXQuestionId = testQuestionInfo.QID,
                        ChoiceId = choices.UserChoices.FirstOrDefault().ChoiceId,
                        MarkScored = 2,
                        Answer = choices.Answer

                    });
                }
                _ctx.SaveChanges();

            }
            //get the next question depending on the direction
            var nextQuestionNumber = 1;
            if (choices.Direction.Equals("forward",StringComparison.CurrentCultureIgnoreCase))
            {
                nextQuestionNumber = _ctx.TestXQuestions.Where(x => x.TestId == choices.TestId
                    && x.QuestionNumber > choices.QuestionId)
                    .OrderBy(x => x.QuestionNumber).Take(1).Select(x => x.QuestionNumber).FirstOrDefault();

            }

            else

            {
                nextQuestionNumber = _ctx.TestXQuestions.Where(x => x.TestId == choices.TestId
                    && x.QuestionNumber < choices.QuestionId)
                    .OrderByDescending(x => x.QuestionNumber).Take(1).Select(x => x.QuestionNumber).FirstOrDefault();

            }
            if (nextQuestionNumber < 1)
                nextQuestionNumber = 1;


            return RedirectToAction("EvalPage", new {
                @token=Session["TOKEN"],
                @qno=nextQuestionNumber});
        }
        
        
        public ActionResult Result(Guid token,ResultModel model)
        {
            
            var _ctx = new OnlineExamEntities4();
            // verify the user is registered and is allowed to check the question
            Registration registration = _ctx.Registrations.Where(x => x.Token==token 
                           ).Single();
            if (token == null)
            {

                TempData["message"] = "this registration is invalid";
                return RedirectToAction("Index");
            }
            else 
            {
                registration.TokenExpireTime = DateTime.Now.TimeOfDay;
                _ctx.SaveChanges();
            }
           
            var test = _ctx.TestXPapers.Where(x => x.RegistrationId == registration.Id).FirstOrDefault();
            if (test != null)
            {
                model.TestId = registration.TestId;
                model.TestName = registration.Test.Name;
                model.UserName = registration.User.Name;
                decimal msum = 0, qsum = 0;
                var mlist=_ctx.TestXPapers.Where(x => x.RegistrationId == registration.Id && x.MarkScored > 0).Select(x=>x.MarkScored).ToList();
                foreach (var m in mlist)
                {
                    msum = msum + m.Value;
                }
                var markscored = msum;
                var qlist = _ctx.TestXQuestions.Where(x => x.TestId == registration.TestId).Select(x => x.Question.Points).ToList();
                foreach (var q in qlist)
                {
                    qsum = qsum + q;
                }
                //ViewBag.totalmarks = 2*(_ctx.Tests.Where(x => x.Id == registration.TestId).Select(x=>x.TestXQuestions.Count()).FirstOrDefault());
                ViewBag.totalmarks = qsum;
                //model.MarkScored = markscored;
                model.MarkScored = msum;
                registration.MarkScored = markscored;
                registration.MaximumMarks=ViewBag.totalmarks;
                _ctx.SaveChanges();

               // to remove textxpapers data after the test
                Registration reg = _ctx.Registrations.Where(x => x.Token == token
                          ).SingleOrDefault();
                var txp = _ctx.TestXPapers.Where(x => x.RegistrationId == registration.Id).ToList();
                if (token == null)
                {

                    TempData["message"] = "this registration is invalid";
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (var t in txp)
                    {
                        _ctx.TestXPapers.Remove(t);
                    }
                    //_ctx.Registrations.Remove(registration); we don't want to remove registration
                    _ctx.SaveChanges();
                }
                
                
                // to send the marks scored by user to his/her email
                sendResultMail(registration);
            }
            
            return View(model);
        }

        // to send result through mail
        private void sendResultMail(Registration user)
        {
            var userRegDetail = db.Registrations.Where(x => x.User.Email == user.User.Email && x.User.Name == user.User.Name && x.Token==user.Token).FirstOrDefault();
            try
            {
                using (MailMessage mm = new MailMessage("onlineexamsystembyankit@gmail.com", user.User.Email))
                {
                    mm.Sender = new MailAddress("onlineexamsystembyankit@gmail.com");
                    mm.Subject = "Result";
                    string body = "Hello <strong>" + user.User.Name + "</strong>,<div><br /><br /><strong>Your scorecard:</strong><br/>Test Name : " + userRegDetail.Test.Name + "<br/>User Name : " + userRegDetail.User.Name + "<br/>Your Score : " + userRegDetail.MarkScored + "/" + userRegDetail.MaximumMarks + "</div> <br /><br />Thanks, <br/>Regards,<br/> Ankit Chauhan";
                    mm.Body = body;
                    mm.IsBodyHtml = true;
                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = "smtp.gmail.com";
                    smtp.EnableSsl = true;
                    NetworkCredential NetworkCred = new NetworkCredential("onlineexamsystembyankit@gmail.com", "OnlineExamSystem123");
                    smtp.UseDefaultCredentials = false;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Credentials = NetworkCred;
                    //smtp.Port = 587;
                    smtp.Send(mm);
                    TempData["message"] = "<strong class='text-danger'>The Result has been sent to your mail successfully!";
                }
            }
            catch (Exception e)
            {
                TempData["message"] = "<strong class='text-primary'>Following exception occured while sending result to mail :</strong> <br/>"+e;
            }
        }


        //(already done in result action) to delete registration and testxpaper details of user after exit  (just replace 'take another test' redirect to Exit(Session[TOKEN]))
        //public ActionResult Exit(Guid token)
        //{

        //    var _ctx = new OnlineExamEntities4();
        //    // verify the user is registered and is allowed to check the question
        //    Registration registration = _ctx.Registrations.Where(x => x.Token == token
        //                   ).SingleOrDefault();
        //    var txp = _ctx.TestXPapers.Where(x => x.RegistrationId == registration.Id).ToList();
        //    if (token == null)
        //    {

        //        TempData["message"] = "this registration is invalid";
        //        return RedirectToAction("Index");
        //    }
        //    else
        //    {
        //        foreach (var t in txp)
        //        {
        //            _ctx.TestXPapers.Remove(t);
        //        }
        //        //_ctx.Registrations.Remove(registration); we don't want to remove registration
        //        _ctx.SaveChanges();
        //    }

        //    return RedirectToAction("Index", "Home");
        //}
        public ActionResult About()
        {
            return View();
        }
    }
}
