using OnlineExam.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace OnlineExam.Controllers
{
    public class AdminController : Controller
    {
        OnlineExamEntities4 db = new OnlineExamEntities4();
        //
        // GET: /Admin/

        
       public ActionResult LogOn()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOn(admindetails model, string returnUrl)
        {
            OnlineExamEntities4 db = new OnlineExamEntities4();

            if (ModelState.IsValid)
            {
                var admin = (from adminlist in db.Admins
                            where adminlist.Name == model.UserName && adminlist.Password == model.Password
                            select new
                            {
                                adminlist.Name
                            }).ToList();

                if (admin.FirstOrDefault() != null)
                {
                    FormsAuthentication.SetAuthCookie(model.UserName, false);
                    return RedirectToAction("Details", "Admin");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid login credentials.");
                }
            }
            return View();
        }

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }


        public ActionResult Details()
        {

            admindetails viewdetails = db.Admins.Where(m => m.Name == User.Identity.Name).ToList().Select(val => new admindetails()
            {
                Id = val.Id,
                UserName = val.Name,
                Password = val.Password
            }).SingleOrDefault();
            return View(viewdetails);
        }

        [HttpGet]
        public ActionResult Edit()
        {
            var adminDetails = (from q in db.Admins.ToList().Where(d => d.Name == User.Identity.Name)
                               select new admindetails
                               {
                                   Id = q.Id,
                                   UserName = q.Name,
                                   Password = q.Password
                               }).FirstOrDefault();
            return View(adminDetails);

        }

        public ActionResult Edit(admindetails mod)
        {
            var user = db.Admins.Where(val => val.Id == mod.Id).SingleOrDefault<Admin>();
            user.Id = mod.Id;
            user.Name = mod.UserName;
            db.SaveChanges();
            if (user.Name != null)
            {
                FormsAuthentication.SetAuthCookie(mod.UserName, false);
                return RedirectToAction("Details", "Admin");
            }
            return View();
        }


        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                bool changePasswordSucceeded;
                try
                {
                    var currentAdmin = db.Admins.Where(val => val.Password == model.OldPassword).SingleOrDefault<Admin>();
                    currentAdmin.Password = model.NewPassword;
                    db.SaveChanges();
                    changePasswordSucceeded = true;
                }
                catch (Exception)
                {
                    changePasswordSucceeded = false;
                }

                if (changePasswordSucceeded)
                {
                    return RedirectToAction("Details");
                }
                else
                {
                    ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                }
            }
            return View(model);
        }

        
        public ActionResult ModifyTest()
        {
            IList<Test> testlist = db.Tests.ToList();
            return View(testlist);

        } 
        public ActionResult ModifyTest1(int id)
        {
            if (id != 0)
            {
                SessionModel model = new SessionModel();
                model.TestId = id;
                return RedirectToAction("EditTest", model);

            }
            return View("ModifyTest");
        }
       

        public ActionResult EditTest(SessionModel model)
        {
            if (model != null)
            {
                var _ctx = new OnlineExamEntities4();
                var test = _ctx.Tests.Where(x => x.Id == model.TestId).FirstOrDefault();
                if (test != null)
                {
                    ViewBag.TestId = test.Id;
                    ViewBag.TestName = test.Name;
                    ViewBag.TestDescription = test.Description;
                    ViewBag.QuestionCount = test.TestXQuestions.Count;
                    ViewBag.TestDuration = test.DurationInMinute;
                    var sum = 0;
                    var mm = test.TestXQuestions.Where(x => x.TestId == test.Id).ToList();
                    foreach (var m in mm)
                    {
                        sum = sum + m.Question.Points;
                    }
                    ViewBag.MM = sum;
                }
                var ques=_ctx.Questions.ToList();
                    IList<Question> testxquestion = (
                        from a in _ctx.TestXQuestions.Where(x => x.TestId == test.Id).AsEnumerable()
                        join b in ques on a.QuestionId equals b.Id
                        select b).ToList();

                    return View(testxquestion);
                
                
            }
            return View(model);
        }

// delete and edit ques in a particular test
        [HttpGet]
        public ActionResult EditQues(int Id)
        {

            var quesDetails = (from q in db.Questions.ToList().Where(d => d.Id == Id)
                               select new Question
                               {
                                   Id = q.Id,
                                   //QuestionCategoryId=q.QuestionCategoryId,
                                   //QuestionType=q.QuestionType,
                                   Question1=q.Question1,
                                   Points=q.Points,
                                   IsActive=q.IsActive
                               }).FirstOrDefault();
            return View(quesDetails);

        }

        [HttpPost]
        public ActionResult EditQues(Question mod)
        {
            if(ModelState.IsValid)
            {
            Question ques = db.Questions.Where(val => val.Id == mod.Id).Single<Question>();
            ques.QuestionCategoryId = 2; //hard coded to remove multiple select options
            ques.QuestionType = "radio";
            //ques.QuestionCategoryId = mod.QuestionCategoryId;
            //if (ques.QuestionCategoryId == 1)
            //{
            //    ques.QuestionType = "mcq";
            //}
            //else
            //{
            //    ques.QuestionType = "radio";
            //}
            ques.Question1 = mod.Question1;
            ques.Points = mod.Points;
            ques.IsActive = mod.IsActive;
            db.SaveChanges();
           var testid= ques.TestXQuestions.Where(x => x.QuestionId == ques.Id).Select(y=>y.TestId).Single();
           SessionModel model = new SessionModel();
           model.TestId = testid;
            return RedirectToAction("EditTest",model);
            }
            return View();
        }

       
        
        public ActionResult Choices(int Id)
        {
            ViewBag.ques = db.Questions.Where(x => x.Id == Id).Select(y => y.Question1).Single();
            ViewBag.quesid = db.Questions.Where(x => x.Id == Id).Select(y => y.Id).Single();
            ViewBag.testid = db.TestXQuestions.Where(x => x.QuestionId == Id).Select(x => x.TestId).First();
           var option = (from q in db.Choices.ToList().Where(d => d.QuestionId == Id)
                          select new QXOModel
                          {
                              ChoiceId = q.Id,
                              Label = q.Label,
                              Points = q.Points,
                              IsActive = q.IsActive
                          }).ToList();
            return View(option);
        }

        [HttpGet]
        public ActionResult EditChoices(int Id)
        {
            var choice = (from q in db.Choices.ToList().Where(d => d.Id == Id)
                               select new Choice
                               {
                                   Id = q.Id,
                                   Label = q.Label,
                                   Points = q.Points,
                                   IsActive = q.IsActive,
                               }).FirstOrDefault();
            return View(choice);
        }

        [HttpPost]
        public ActionResult EditChoices(Choice mod)
        {
            var choice = db.Choices.Where(val => val.Id == mod.Id).SingleOrDefault();
            choice.Id = mod.Id;
            choice.Label = mod.Label;
            choice.Points = mod.Points;
            choice.IsActive = mod.IsActive;
            mod.QuestionId = choice.QuestionId;
            db.SaveChanges();
            
            var quesid = db.TestXQuestions.Where(x => x.QuestionId == mod.QuestionId).Select(y=>y.QuestionId).Single();
            Question model = new Question();
            model.Id = quesid;
            return RedirectToAction("Choices", model);
        }

        public ActionResult DeleteChoices(int Id)
        {
            var delete = (from q in db.Choices.ToList().Where(d => d.Id == Id)
                          select new Choice
                          {
                              Id = q.Id,
                              Label = q.Label,
                              Points = q.Points,
                              IsActive = q.IsActive,
                          }).FirstOrDefault();
            return View(delete);
        }

        [HttpPost] 
        public ActionResult DeleteChoices(Choice mod)
        {
            Choice choicedelete = db.Choices.Where(val => val.Id == mod.Id).Single<Choice>();
            mod.QuestionId = choicedelete.QuestionId;
            db.Choices.Remove(choicedelete);
            db.SaveChanges();
            
            var quesid = db.TestXQuestions.Where(x => x.QuestionId == mod.QuestionId).Select(y => y.QuestionId).Single();
            Question model = new Question();
            model.Id = quesid;
            return RedirectToAction("Choices", model);
        }  


        public ActionResult DeleteQues(int id)
        {
            var delete = db.Questions.ToList().Where(val => val.Id == id).Select(val => new Question()
            {
                Id = val.Id,
                QuestionCategoryId = val.QuestionCategoryId,
                QuestionType = val.QuestionType,
                Question1 = val.Question1,
                Points = val.Points,
                IsActive = val.IsActive
            }).SingleOrDefault();

            return View(delete);
        }

        [HttpPost]
        public ActionResult DeleteQues(Question mod)
        {
            Question quesdelete = db.Questions.Where(val => val.Id == mod.Id).Single<Question>();
            var testid = quesdelete.TestXQuestions.Where(x => x.QuestionId == quesdelete.Id).Select(y => y.TestId).Single();
            var choice = db.Choices.Where(x => x.QuestionId == quesdelete.Id).ToList();
            foreach (var c in choice)
            {
                db.Choices.Remove(c);
            }
            TestXQuestion txq=db.TestXQuestions.Where(x=>x.QuestionId==quesdelete.Id).Single<TestXQuestion>();
            db.TestXQuestions.Remove(txq);
            db.Questions.Remove(quesdelete);
            db.SaveChanges();


            // to reset the sequence of the question number in database
            var queslist = db.TestXQuestions.Where(x => x.TestId == testid).ToList();
            for (int t = 0; t < queslist.Count();t++ )
            {
                queslist[t].QuestionNumber = t + 1;
                db.SaveChanges();
            }
            

            SessionModel model = new SessionModel();
            model.TestId = testid;
            return RedirectToAction("EditTest",model);
        }  

// add new question  or choice to a test selected
        public ActionResult AddQues(int id)
        {
            var test = db.Tests.Where(x =>x.Id==id).Select(x=>x.Id).Single();
            AddQues testid = new AddQues();
            testid.TestId = test;
            return View(testid);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddQues(AddQues m)
        {
            if (ModelState.IsValid)
            {
                if (m != null)
                {
                    Question nq = new Question();
                    nq.QuestionCategoryId = 2;  // hard coded to remove multiple option select 
                    nq.QuestionType = "radio";
                    //nq.QuestionCategoryId = m.QuestionCategoryId;
                    //if (nq.QuestionCategoryId == 1)
                    //    nq.QuestionType = "mcq";
                    //else
                    //    nq.QuestionType = "radio";
                    nq.Question1 = m.Question;
                    nq.Points = Convert.ToInt32(m.Points);
                    nq.IsActive = m.IsActive;

                    db.Questions.Add(nq);
                    
                    db.SaveChanges();
                    
                    
                    var testid = m.TestId;
                    TestXQuestion newTXQ = new TestXQuestion();
                    newTXQ.TestId = testid;
                    newTXQ.QuestionId = nq.Id;
                    var z=db.TestXQuestions.Where(p=>p.TestId==testid).Select(p=>p.QuestionNumber).FirstOrDefault();
                    if (z!= 0)
                    {
                        newTXQ.QuestionNumber = db.TestXQuestions.Where(x => x.TestId == testid).Select(x => x.QuestionNumber).Max() + 1;
                    }
                    else
                    {
                        newTXQ.QuestionNumber = 1;
                    }
                    newTXQ.IsActive = nq.IsActive;
                    db.TestXQuestions.Add(newTXQ);
                    db.SaveChanges();

                    return RedirectToAction("AddChoice", "Admin", new {@id= nq.Id });
                }
            }
            return View();
        }
        
        public ActionResult AddChoice(int id)
        {
            ViewBag.ques = db.Questions.Where(x => x.Id == id).Select(y => y.Question1).Single();
            Choice newchoice = new Choice();
            newchoice.QuestionId = id;
            return View(newchoice);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddChoice(Choice m)
        {
            if (ModelState.IsValid)
            {
                if (m != null)
                {
                    var choice = new Choice();
                    choice.QuestionId = m.Id;
                    choice.Label = m.Label;
                    choice.Points = m.Points;
                    choice.IsActive = m.IsActive;
                    db.Choices.Add(choice);
                    db.SaveChanges();

                    var quesid = db.TestXQuestions.Where(x => x.QuestionId == m.Id).Select(y => y.QuestionId).Single();
                    Question model = new Question();
                    model.Id = quesid;
                    return RedirectToAction("Choices", model);
                }
            }
            return View();
        }

        public ActionResult AddTest()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddTest(AddTest m)
        {
            ModelState.Remove("Id");
            if (ModelState.IsValid)
            {
                Test test = new Test();
                test.Name = m.Name;
                test.Description = m.Description;
                test.IsActive = m.IsActive;
                test.DurationInMinute = m.DurationInMinute;
                db.Tests.Add(test);
                db.SaveChanges();
                SessionModel model = new SessionModel();
                model.TestId = test.Id;
                return RedirectToAction("EditTest", model);
            }
            return View();
        }

        [HttpGet]
        public ActionResult DeleteTest(int id)
        {
            if (id != 0)
            {
                var test = db.Tests.Where(x => x.Id == id).Single();
                return View(test);
            }
            return View();
        }
        [HttpPost]
        public ActionResult DeleteTest(Test t)
        {
            if (t != null)
            {
                var dt = db.Tests.Where(x => x.Id == t.Id).Single();
                var dq = db.TestXQuestions.Where(x => x.TestId == t.Id).ToList();
                foreach (var p in dq)
                {
                    var q = db.Questions.Where(x => x.Id == p.QuestionId).ToList();
                        foreach(var z in q)
                        {
                            var c = db.Choices.Where(x => x.QuestionId == p.QuestionId).ToList();
                            foreach (var y in c)
                            {
                                db.Choices.Remove(y);
                            }
                        db.Questions.Remove(z);
                        }
                    db.TestXQuestions.Remove(p);
                }
              
                db.Tests.Remove(dt);
                db.SaveChanges();
                return RedirectToAction("ModifyTest");
            }
            return View();

        }

        [HttpGet]
        public ActionResult EditTestDetails(int id)
        {
            if (id != 0)
            {
                var test = db.Tests.Where(x => x.Id==id).Single();
                return View(test);
            }
            return View();
        }
        [HttpPost]
        public ActionResult EditTestDetails(Test t)
        {
            if (t != null)
            {
                var test=db.Tests.Where(x=>x.Id==t.Id).Single();
                test.Name = t.Name;
                test.Description = t.Description;
                test.DurationInMinute = t.DurationInMinute;
                test.IsActive = t.IsActive;
                db.SaveChanges();
                return RedirectToAction("ModifyTest");
            }
            return View();
        }
    }
}
