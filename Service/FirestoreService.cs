// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Backend.Models;
// using Google.Cloud.Firestore;

// namespace Backend.Service
// {
//     public class FirestoreService : IFirestoreService
//     {
//         private readonly FirestoreDb firestoreDb;

//         public FirestoreService(string projectId)
//         {
//             firestoreDb = FirestoreDb.Create(projectId, null);
//         }

//         public async Task StoreUserAsync(User user)
//         {
//             var userRef = firestoreDb.Collection("DB_Users").Document(user.Id);
//             var userData = new
//             {
//                 FirstName = user.FirstName,
//                 LastName = user.LastName,
//                 Email = user.Email,
//                 DateCreated = user.DateCreated,
//                 UserName = user.UserName,
//                 // Add any additional fields if necessary
//             };
//             await userRef.SetAsync(userData);
//         }

//         public async Task<User?> GetUserAsync(string userId)
//         {
//             var userRef = firestoreDb.Collection("DB_Users").Document(userId);
//             var userDoc = await userRef.GetSnapshotAsync();

//             if (userDoc.Exists)
//             {
//                 var userData = userDoc.ConvertTo<User>();
//                 return userData;
//             }

//             return null;
//         }
//     }
// }
