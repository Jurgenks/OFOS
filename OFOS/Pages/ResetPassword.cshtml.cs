using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OFOS.Pages
{
    public class ResetPasswordModel : PageModel
    {
        public string Token { get; set; }
        public string Message { get; set; }

        //public async Task<IActionResult> OnPostAsync(string NewPassword, string token)
        //{
        //    if (string.IsNullOrEmpty(NewPassword))
        //    {
        //        ModelState.AddModelError("NewPassword", "Please provide a new password");
        //        return Page();
        //    }

        //    // Reset password with user, token and new password
        //    var result = await _userService.ResetPasswordWithToken(token, NewPassword);

        //    if (result.Succeeded)
        //    {
        //        // Password reset was successful, redirect to login page
        //        return RedirectToPage("/Login");
        //    }
        //    else
        //    {
        //        // Password reset failed, display error message to user
        //        ModelState.AddModelError(string.Empty, "An error occurred while resetting your password");
        //        return Page();
        //    }
        //}

    }
}
