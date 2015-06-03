<?php


namespace PHPAnalyzer\Commands;


use Symfony\Component\Console\Command\Command;
use Symfony\Component\Console\Input\InputArgument;
use Symfony\Component\Console\Input\InputInterface;
use Symfony\Component\Console\Input\InputOption;
use Symfony\Component\Console\Output\OutputInterface;

class ScanCommand extends Command{
    protected function configure() {
        $this->setName('scan')
             ->setDescription('Start PHP vulnerability scanning')
             ->addArgument('target', InputArgument::REQUIRED, 'Target file/directory of scan.');
    }

    protected function execute(InputInterface $input, OutputInterface $output) {
        $target = $input->getArgument('target');

        if (is_dir($target)) {
            $output->writeln("Starting directory scan of " . $target);
        } else if (is_file($target)) {
            $output->writeln("Starting file scan of " . $target);
        } else {
            $output->writeln("Target does not seem to be a valid file or directory..");
        }
    }
}