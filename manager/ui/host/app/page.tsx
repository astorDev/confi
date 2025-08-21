import { Button } from "@/components/ui/button"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog"

export default function Home() {
  return (
    <div className="font-sans grid grid-rows-[20px_1fr_20px] items-center justify-items-center min-h-screen p-8 pb-20 gap-16 sm:p-20">
      <main className="flex flex-col gap-[32px] row-start-2 items-center">
        <h1 className="text-xl">🚧 Here we will build the Confi UI 🚧</h1>
        <Dialog>
          <DialogTrigger asChild>
            <Button>Button demonstrating shadcn/ui</Button>
          </DialogTrigger>
          <DialogContent>
            <DialogHeader>
              <DialogTitle className="text-center text-2xl">
                🎉 Congrats! 
              </DialogTitle>
              <DialogDescription className="text-center text-lg mt-4">
                 You've found an easter egg! 🐣 
              </DialogDescription>
            </DialogHeader>
          </DialogContent>
        </Dialog>
      </main>
    </div>
  );
}
