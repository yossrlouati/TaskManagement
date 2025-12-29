using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleAuthApi.Data;
using SimpleAuthApi.Dto;
using SimpleAuthApi.Models;
using SimpleAuthApi.Services;
using System.Security.Claims;

namespace SimpleAuthApi.Controllers
{
    [Authorize] //Exige que user soit conncté (jwt token valide)
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        //BASE
        private readonly AppDbContext _context; // 5. Variable privée pour le contexte de DB
        private readonly IEmailService _emailService; // Ajout du champ pour le service d'email
        private readonly UserManager<IdentityUser> _userManager; // Ajout pour chercher l'email
        // Constructeur (Injection de Dépendance)
        public TasksController(AppDbContext context, IEmailService emailService, // Nouveau service injecté
        UserManager<IdentityUser> userManager) // Service UserManager injecté)
        {

            _context = context; // 6. Initialise la variable _context avec le contexte DB injecté par le framework.
            _emailService = emailService;
            _userManager = userManager;
        }
        //GET -LINQ
        /*[HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetMyTasks() //Task : objet de type task avec code Http (ActionResult) et corps TaskItem en cas de succés
        {
            //Recupérer l 4id de user à partir de jwt token
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier); //User vient de la classe de base ControllerBase

            //User représente l'objet Principal (l'identité) de l'utilisateur ayant fait la requête. Il est créé par le middleware d'authentification JWT après avoir validé le token
            
           // Lorsque vous utilisez[Authorize], le framework s'assure que l'objet User existe, et vous utilisez ses méthodes pour récupérer l'ID de l'utilisateur connecté.
            if (currentUserId == null)
            {
                return Unauthorized(); //401
            }
            //LINQ : interrogation async de la BD 
            var userTasks = await _context.TaskAssignements
                .Where(ta => ta.UserId == currentUserId) //filtrer selon user id
                .Select(ta => ta.Task)
                .Include(t => t.Assignements)
                .ToListAsync(); //List de user tasks
            //Any() methode de Linq pour les collections (list..), test si contient au moins un element donc true
            if (!userTasks.Any()) //liste vide donc false ; on fait !false qui donne true pour retourner 404 not found
            {
                return NotFound("Aucunetache trouvée pour cet utilisateur");
            }
            return Ok(userTasks); //liste de tache code 200


        }*/

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<TaskItem>> CreateTask(TaskItem task, [FromQuery] List<string> assignedUserIds)// Pour une transaction atomique
        {
            // FromQuery : lue à partir de l'url
            //recupere l id de l admin
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (adminId == null) return Unauthorized();
            var adminUser = await _userManager.FindByIdAsync(adminId);
           
            string adminName = adminUser?.UserName ?? "Administrateur Inconnu";
            //
            task.AssignedByAdminId = adminId;
            _context.TaskItems.Add(task);
            await _context.SaveChangesAsync();
            //attribution
            var assignements = assignedUserIds.Select(userId => new TaskAssignement
            {
                TaskId = task.Id,
                UserId = userId,
                AssignedDate = DateTime.Now

            }).ToList();
            _context.TaskAssignements.AddRange(assignements);
            await _context.SaveChangesAsync(); //EF core methodes Linq
                                               // 2. LOGIQUE D'ENVOI D'EMAIL POUR CHAQUE UTILISATEUR
            /*foreach(var userId in assignedUserIds)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user?.Email != null)
                {
                    await _emailService.SendAssignementNotifiactionAsync(user.Email, task.Title, adminName);
                }
            }*/
            // 2. Préparation des emails (on récupère les emails des users assignés)
            /* var emails = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => u.Email)
                .ToListAsync(); // Ici on utilise ToListAsync car c'est la fin de la requête [cite: 43]

            // 3. Déclenchement de l'email
            string? fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", task.DocumentPath);
            await _emailService.SendTaskNotificationAsync(emails, task.Title, fullPath);*/
            // 1. Récupérer les emails des utilisateurs sélectionnés
            var emails = new List<string>();
            foreach (var userId in assignedUserIds) // Correction du nom de la variable ici
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user?.Email != null)
                {
                    emails.Add(user.Email);
                }
            }

            // 2. Préparer le chemin du document (s'il existe)
            string? fullPath = null;
            if (!string.IsNullOrEmpty(task.DocumentPath))
            {
                fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", task.DocumentPath);
            }

            // 3. Un seul appel au service (plus performant)
            if (emails.Any())
            {
                // On passe la liste d'emails et le chemin du fichier
                await _emailService.SendTaskNotificationAsync(emails, task.Title, fullPath, adminId);
            }

            return CreatedAtAction(
                nameof(GetTasks), // 1. Nom de la méthode qui permet de récupérer l'objet (ici, la méthode GET)
                new { id = task.Id }, // 2. Les paramètres nécessaires pour cette méthode (l'ID de la tâche créée)
                task // 3. L'objet créé à retourner dans le corps de la réponse
                );
            //Le But (Convention REST) : Lorsqu'une ressource est créée (statut HTTP 201 Created), la convention REST exige que
            //l'API fournisse au client comment accéder à cette nouvelle ressource
        }
        //VISUALISATION DES TACHES (ADMIN & EMPLOYER)
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks()
        {
            //ID et Role
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null)
            {
                return Unauthorized();
            }
            bool isAdmin = User.IsInRole("Admin");
            //Démarrer la requete selon le role et l'id
            IQueryable<TaskItem> tasksQuery = _context.TaskItems;
            if (isAdmin)
            {
                tasksQuery = tasksQuery.Where(t => t.AssignedByAdminId == currentUserId);
                /*if (!userTasks.Any()) //liste vide donc false ; on fait !false qui donne true pour retourner 404 not found
                {
                    return NotFound("Aucunetache trouvée pour cet utilisateur");
                }*/
            }
            else // C'est un simple Employé
            {
                // CAS EMPLOYÉ : Filtre les tâches qui lui sont assignées.
                // On passe par la table de liaison TaskAssignement.
                tasksQuery = tasksQuery.Where(t => t.Assignements.Any(a => a.UserId == currentUserId));
            }
            var userTasks = await tasksQuery
                .Include(t => t.Assignements)
                .ToListAsync();

            if (!userTasks.Any())
            {
                return NotFound("Aucune tâche trouvée.");
            }

            // Le statut 200 OK et la liste des tâches
            return Ok(userTasks);



        }

        /*[HttpPut("{id}")]
        [Authorize("Admin")]
        public async Task<IActionResult> PutTaskItem(int id, TaskItem updatedTask)
        {
            if (id != updatedTask.Id)
            {
                return BadRequest();
            }

            // Assurez-vous que seule la date, le titre et la description sont modifiables par cette méthode.
            // Les attributions pourraient nécessiter un endpoint PATCH séparé.

            _context.Entry(updatedTask).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Vérification de l'existence de la tâche
                if (!_context.TaskItems.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();


        }*/
        //Modification de tache (Update)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        //FromForm pour accepter Iformfile et d'autres données
        public async Task<IActionResult> PutTaskItem(int id, [FromForm] TaskUpdateDto model)
        {
            var task = await _context.TaskItems.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }
            //Mise à jour des donnees simple 
            task.Title = model.Title;
            task.Description = model.Description;
            task.DueDate = model.DueDate;
            //Document :Remplacement et suppression
            var oldPath = task.DocumentPath;
            if (model.DeleteExistingDocument || model.File != null)
            {
                if (!string.IsNullOrEmpty(oldPath))
                {
                    var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", oldPath.TrimStart('/'));
                    if (System.IO.File.Exists(absolutePath))
                    {
                        System.IO.File.Delete(absolutePath);//Suppression physique
                    }

                }
                task.DocumentPath = null; // Réinitialisation dans la DB
            }
            if (model.File != null)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Documents");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                var fileName = $"{Guid.NewGuid()}_{model.File.FileName}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.File.CopyToAsync(stream);
                }
                task.DocumentPath = $"/Documents/{fileName}";
            }
            await _context.SaveChangesAsync();
            return NoContent();
        }

        //Modification des attribution 
        // Dans TasksController.cs
        [HttpPatch("{taskId}/assignments")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTaskAssignments(int taskId, [FromBody] UpdateAssignmentsDto model)
        {
            var task = await _context.TaskItems
                .Include(t => t.Assignements) // IMPORTANT: Chargez les attributions actuelles
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                return NotFound();
            }

            // 1. Identifier les attributions à supprimer (celles qui ne sont plus dans la liste fournie)
            var assignmentsToRemove = task.Assignements
                .Where(a => !model.UserIds.Contains(a.UserId))
                .ToList();

            _context.TaskAssignements.RemoveRange(assignmentsToRemove);

            // 2. Identifier les ID à ajouter (ceux qui sont dans la liste mais pas encore en DB)
            var existingUserIds = task.Assignements.Select(a => a.UserId).ToList();
            var userIdsToAdd = model.UserIds.Except(existingUserIds).ToList();

            // 3. Créer et ajouter les nouvelles attributions
            foreach (var userId in userIdsToAdd)
            {
                task.Assignements.Add(new TaskAssignement
                {
                    TaskId = taskId,
                    UserId = userId,
                    AssignedDate = DateTime.Now // Assurez-vous d'initialiser AssignedDate
                });

                // OPTIONNEL : Envoyer l'email de notification aux nouveaux utilisateurs assignés ici
                // var user = await _userManager.FindByIdAsync(userId);
                // await _emailService.SendEmailAsync(...)
            }

            await _context.SaveChangesAsync();

            return NoContent(); // Code 204
        }
        //SUPPRESSION DE TACHE
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTaskItem(int id)
        {
            var taskItem = await _context.TaskItems.FindAsync(id);
            if (taskItem == null)
            {
                return NotFound();

            }
            // NOUVEAU: Gestion du document physique lié
            if (!string.IsNullOrEmpty(taskItem.DocumentPath))
            {
                var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", taskItem.DocumentPath.TrimStart('/'));

                if (System.IO.File.Exists(absolutePath))
                {
                    System.IO.File.Delete(absolutePath); // Suppression physique du fichier
                }
            }
            // Fin de la gestion du document
            _context.Remove(taskItem);
            await _context.SaveChangesAsync();
            return NoContent();// code 204
        }

        //Marque une tache complete
        [HttpPatch("{id}/complete")]
        [Authorize]
        public async Task<IActionResult> MarkTaskAsComplete(int id, [FromBody] TaskStatusUpdatedDto statusUpdate)
        {
            var task = await _context.TaskItems
                .Include(t => t.Assignements)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (task == null)
            {
                return NotFound();
            }
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isAdmin = User.IsInRole("Admin");
            bool isAssigned = task.Assignements.Any(a => a.UserId == currentUserId);
            //Interdiction
            if (!isAdmin && !isAssigned)
            {
                return Forbid(); // Code 403: L'utilisateur n'a pas la permission
            }

            task.IsCompleted = statusUpdate.IsCompleted;
            await _context.SaveChangesAsync();

            // Vous pouvez envoyer une notification à l'Admin ici si IsCompleted devient true
            // await _emailService.SendEmailAsync(...)

            return NoContent();
        }
        //UPLOAD FILE
        [HttpPost("{taskId}/upload")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadDocument(int taskId, [FromForm] DocumentUploadDto model)
        {
            var task = await _context.TaskItems.FindAsync(taskId);
            if (task == null) return NotFound();
            //Chemin de sauvegarde
            var fileName = $"{Guid.NewGuid()}_{model.File.FileName}";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Documents", fileName);

            //sauvegarde dan le serveur 
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.File.CopyToAsync(stream);
            }
            //mettre à jour le chemin dans la BD
            task.DocumentPath = $"/Documents/{fileName}";
            await _context.SaveChangesAsync();
            return Ok(new { Path = task.DocumentPath });
        }
        //Download Document
        [HttpGet("{taskId}/download")]
        [Authorize] // L'Admin ET l'Employé doivent être autorisés
        public async Task<IActionResult> DownloadDocument(int taskId)
        {
            var task = await _context.TaskItems.FindAsync(taskId);
            if (task == null || string.IsNullOrEmpty(task.DocumentPath))
            {
                return NotFound();
            }

            // Sécurité : Optionnel, mais vous pouvez vérifier que l'employé est assigné à la tâche
            /*
             * // LOGIQUE DE VÉRIFICATION D'ACCÈS
             var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
             bool isAdmin = User.IsInRole("Admin");
             bool isAssigned = task.Assignements.Any(a => a.UserId == currentUserId);

             if (!isAdmin && !isAssigned)
             {
              // Si l'utilisateur n'est ni Admin, ni assigné à la tâche, accès interdit (403)
               return Forbid(); 
              }
             */
            //Chemin absolu sur le serveur 
            var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", task.DocumentPath.TrimStart('/'));
            if (!System.IO.File.Exists(absolutePath))
            {
                return NotFound();
            }
            var fileStream = System.IO.File.OpenRead(absolutePath);
            var mimeType = "application/octet-stream"; // Type générique, à améliorer si besoin
            return File(fileStream, mimeType, Path.GetFileName(absolutePath));


        }
    







    




    }
  


    
}
