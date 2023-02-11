using BingoVintage.Models;
using Dapper;
using System.Data.SqlClient;

namespace BingoVintage.Data
{
    public class UsrData
    {
        private readonly string _cnn;
        public UsrData() 
        {
            var getConection = new GetConnectionString();
            _cnn = getConection.ConnectionString("BingoVintage");
        }

        //Method Search Email in DB - LOGIN.
        public Usr FindUsrByEmail(string e)
        {
            using var cnn = new SqlConnection(_cnn);
            cnn.Open();
            var sql = "SELECT * FROM dbo.Users WHERE Email = @e";
            return cnn.QueryFirstOrDefault<Usr>(sql, new {e});
        }
        
        //Method New Usr.
        public int? CreateUsr(Usr u)
        {
            using var cnn = new SqlConnection(_cnn);
            cnn.Open();
            //OUTPUT INSERTED.ID devuelve el id de el usuario recien creado.
            var sql = "INSERT INTO BingoVintage.dbo.Users( Name, Email, Token, PlusEighteen, LastDayOnline) OUTPUT INSERTED.USRID VALUES (@Name,@Email,@Token, @PlusEighteen , @LastDayOnline)";
            return cnn.QueryFirstOrDefault<int?>(sql, u);
        }
        
        //Verify UsrByToken.
        public Usr Verified (string t)
        {
            try
            {
                using var cnn = new SqlConnection(_cnn);
                cnn.Open();
                var sql = "SELECT TOP(1) [Email], [Name] from [BingoVintage].[dbo].[Users] where Token = @t";
                return cnn.QuerySingle<Usr>(sql, new { t }); 
            }
            catch (InvalidOperationException)
            {
                return null!;
            }
        }
        
        public Usr? Activate(string e , DateTime d)
        {
            using var cnn = new SqlConnection(_cnn);
            cnn.Open();
            var sql = "UPDATE[BingoVintage].[dbo].[Users] SET Verified = 1, LastDayOnline = @d WHERE Email = @e";
            return cnn.Query<Usr>(sql,new {e, d}).FirstOrDefault();
        }

        //Compare Cookie id and Cookie email to db
        public int GetUsrId(int id)
        {
            try
            {
                using var cnn = new SqlConnection(_cnn);
                cnn.Open();
                var sql = "SELECT TOP(1) [UsrID] from [BingoVintage].[dbo].[Users] where UsrID = @id";
                return cnn.QuerySingle<int>(sql, new { id });
            }
            catch (Exception)
            {
                return 0;
            }   
        }
    }
}