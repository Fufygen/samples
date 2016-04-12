import pygame
import struct
from clients import UDP_client

# Client settings
IP = '192.168.1.56'
PORT = 6000

# Define some colors
BLACK    = (   0,   0,   0)
WHITE    = ( 255, 255, 255)

# This is a simple class that will help us print to the screen
# It has nothing to do with the joysticks, just outputing the
# information.
class TextPrint:
    def __init__(self):
        self.reset()
        self.font = pygame.font.Font(None, 20)

    def _print(self, screen, textString):
        textBitmap = self.font.render(textString, True, BLACK)
        screen.blit(textBitmap, [self.x, self.y])
        self.y += self.line_height
        
    def reset(self):
        self.x = 10
        self.y = 10
        self.line_height = 15
        
    def indent(self):
        self.x += 10
        
    def unindent(self):
        self.x -= 10
    

pygame.init()
 
# Set the width and height of the screen [width,height]
size = [500, 700]
screen = pygame.display.set_mode(size)

pygame.display.set_caption("My Game")

#Loop until the user clicks the close button.
done = False

# Used to manage how fast the screen updates
clock = pygame.time.Clock()

# Initialize the joysticks
pygame.joystick.init()
    
# Get ready to print
textPrint = TextPrint()

client = UDP_client(IP, PORT)

# -------- Main Program Loop -----------
while done==False:
    # EVENT PROCESSING STEP
    for event in pygame.event.get(): # User did something
        if event.type == pygame.QUIT: # If user clicked close
            done=True # Flag that we are done so we exit this loop
        
        # Possible joystick actions: JOYAXISMOTION JOYBALLMOTION JOYBUTTONDOWN JOYBUTTONUP JOYHATMOTION
        if event.type == pygame.JOYBUTTONDOWN:
            print("Joystick button pressed.")
        if event.type == pygame.JOYBUTTONUP:
            print("Joystick button released.")
            
 
    # DRAWING STEP
    # First, clear the screen to white. Don't put other drawing commands
    # above this, or they will be erased with this command.
    screen.fill(WHITE)
    textPrint.reset()

    # Get joystick data
    joystick_count = pygame.joystick.get_count()
    textPrint._print(screen, "Number of joysticks: {}".format(joystick_count) )
    textPrint.indent()
    joystick = pygame.joystick.Joystick(0)
    joystick.init()
    textPrint._print(screen, "Joystick {}".format(0) )
    textPrint.indent()
    name = joystick.get_name()
    textPrint._print(screen, "Joystick name: {}".format(name) )
    axis_lr = joystick.get_axis(0)
    axis_fb = joystick.get_axis(1)
    textPrint._print(screen, "Axis {} value: {:>6.3f}".format(0, axis_lr) )
    textPrint._print(screen, "Axis {} value: {:>6.3f}".format(1, axis_fb) )
    button_fire = joystick.get_button(0)
    textPrint._print(screen, "Button {:>2} value: {}".format(0,button_fire) )
    
    if button_fire:
        client.send_msg(b'/1/fi\0\0\0')
    else:
        client.send_msg(b'/1/st\0\0\0')
    
    client.send_msg(b'/1/fb\0\0\0\0\0\0\0' + struct.pack('>f', axis_fb))
    client.send_msg(b'/1/lr\0\0\0\0\0\0\0' + struct.pack('>f', -axis_lr))
    
    # Go ahead and update the screen with what we've drawn.
    pygame.display.flip()

    # Limit to 20 frames per second
    clock.tick(10)
    
# Close the window and quit.
# If you forget this line, the program will 'hang'
# on exit if running from IDLE.
pygame.quit ()
client.close()