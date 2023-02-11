using BingoVintage.Data;
using BingoVintage.ExtentionMethods;
using BingoVintage.Models;

namespace BingoVintage.Rules
{
    public class UsrRule
    {
        public Response CreateUsr(Usr u)
        {
            try
            {
                //Usr has eighteen?.
                if (u.PlusEighteen == false)
                {
                    return new Response
                    {
                        Code = "72",
                        Message = "You must be 18+",
                        Obj = null,
                        Result = false
                    };
                }
                if (!u.Email.IsEmailValid())
                {
                    return new Response
                    {
                        Code = "70",
                        Message = "Invalid Email",
                        Obj = null,
                        Result = false
                    };
                }
                //Email is in DB?
                var data = new UsrData();
                var uExist = data.FindUsrByEmail(u.Email);
                if (uExist != null)
                {
                    return new Response()
                    {
                        Code = "71",
                        Message = "User Exist",
                        Obj = new { uExist.Name, uExist.Email, uExist.DateOfCreation }, //Anonim Object.
                        Result = false
                    };
                }
                //New Usr.
                var rndT = new UsrMethod();
                var newUsr = new Usr()
                {
                    Email = u.Email,
                    Name = u.Name,
                    PlusEighteen = u.PlusEighteen,
                    Token = rndT.TokenGenerator(10),
                    LastDayOnline = u.LastDayOnline
                };
                try
                {
                    //Send Email Token
                    new EmailMethod().SendEmail(newUsr.Email, newUsr.Name, newUsr.Token); //IMPORTANT - If u have problems sending mails , disable your antivirus or coment this line of code and get the token from DB.

                    //Create usr on DB.
                    var id = data.CreateUsr(newUsr);
                    if (id != null)
                    {
                        return new Response()
                        {
                            Code = "20",
                            Message = "We send you a Token via email - Insert it to validate your acount",
                            Obj = new { newUsr.Email, newUsr.Name },
                            Result = true
                        };
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                catch (Exception)
                {
                    return new Response()
                    {
                        Code = "21",
                        Message = "User was NOT Created",
                        Obj = new { newUsr.Email, newUsr.Name },
                        Result = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new Response()
                {
                    Code = "69",
                    Message = "Error: " + ex.Message,
                    Obj = new { u.Email },
                    Result = false
                };
            }
        }
        public Response Login(Usr u)
        {
            try
            {
                //For Secure the DB
                if (!u.Email.IsEmailValid())
                {
                    return new Response
                    {
                        Code = "70",
                        Message = "Invalid Email",
                        Obj = null,
                        Result = false
                    };
                }

                //User exist?.
                var data = new UsrData().FindUsrByEmail(u.Email);
                if (data == null)
                {
                    return new Response()
                    {
                        Code = "73",
                        Message = "User Not Exist",
                        Obj = null,
                        Result = false
                    };
                }

                //Valid Email so is verified?.
                try
                {
                    if (!data.Verified)
                    {
                        //Token isn´t empty?
                        if (u.Token != string.Empty)
                        {
                            //Verify token with DB
                            var verified = new UsrData().Verified(u.Token);
                            if (verified != null)
                            {
                                var activate = new UsrData().Activate(u.Email, u.LastDayOnline);//Rows afected
                                return new Response()
                                {
                                    Code = "22",
                                    Message = "Validate Success - Please Log In",
                                    Obj = new { verified.Name, verified.Email },
                                    Result = true
                                };
                            }
                            else
                            {
                                return new Response()
                                {
                                    Code = "12",
                                    Message = "Invalid Token",
                                    Obj = new { data.Name, data.Email },
                                    Result = false
                                };
                            }
                        }
                        return new Response()
                        {
                            Code = "11",
                            Message = "Not verified User - Insert Token",
                            Obj = new { data.Name, data.Email }, //Objeto Anñonimo -envío el nombre y el email para que ingrese el token.
                            Result = false
                        };
                    }
                    else
                    {
                        if (u.Token != string.Empty)
                        {
                            return new Response()
                            {
                                Code = "74",
                                Message = "Invalid access.",
                                Obj = null,
                                Result = false
                            };
                        }
                        else
                        {
                            //Login.
                            return new Response()
                            {
                                Code = "10",
                                Message = $"Welcome {data.Name}",
                                Obj = new Usr() { UsrID = data.UsrID, Name = data.Name, Email = data.Email, Verified = data.Verified, DateOfCreation = data.DateOfCreation, PlusEighteen = data.PlusEighteen, Token = data.Token , RememberLogin = data.RememberLogin},
                                Result = true
                            };
                        }
                    }
                }
                //Inexistent Email - Send Usr to index.
                catch (Exception)
                {
                    return new Response()
                    {
                        Code = "72",
                        Message = "User Not Exist",
                        Obj = new { u.Email, u.Name },
                        Result = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new Response()
                {
                    Code = "69",
                    Message = "Error: " + ex.Message,
                    Obj = new { u.Email },
                    Result = false
                };
            }
        }
    }
}